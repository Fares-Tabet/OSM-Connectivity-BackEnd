using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace OSM_Connecitvity_Backend_Revised
{
    public class FileProcessor
    {
        Dictionary<string, Node> NodeDictionary;
        Dictionary<string, Way> WayHashMap;
        Dictionary<string, JunctionNode> JunctionNodeHashMap;

        public FileProcessor()
        {
            NodeDictionary = JsonConvert.DeserializeObject<Dictionary<string,Node>>(File.ReadAllText(@"NodeDictionary.json"));
            WayHashMap = JsonConvert.DeserializeObject<Dictionary<string, Way>>(System.IO.File.ReadAllText(@"ways.json"));
            JunctionNodeHashMap = JsonConvert.DeserializeObject<Dictionary<string, JunctionNode>>(System.IO.File.ReadAllText(@"junctionNodes.json"));
        }

        //finding incorrect highway = motorway connections
        public void generateIncorrectMotorwayConnections(string fileName)
		{
            List<DisconnectionNode> disconnectionNodes = new List<DisconnectionNode>();

            //for each junction node
            foreach (KeyValuePair<string, JunctionNode> node in JunctionNodeHashMap)
			{   //if it is connected to a motorway 
				if (node.Value.roadTypes.Contains("motorway"))
				{
                    int res = (from x in node.Value.roadTypes
                               select x).Distinct().Count();
					if ((res >= 3) || (res==2 && !node.Value.roadTypes.Contains("motorway_link")) || (res==1 && node.Value.roadTypes.Count==1))
					{
                        DisconnectionNode disconnectionNode = new DisconnectionNode();
                        disconnectionNode.Id = node.Value.Id;
                        disconnectionNode.Lat = node.Value.Lat;
                        disconnectionNode.Lng = node.Value.Lng;
                        List<Way> w = new List<Way>();
                        foreach(KeyValuePair<string,string> road in node.Value.wayToNodeMap)
						{
                            w.Add(WayHashMap.GetValueOrDefault(road.Key));
						}
                        disconnectionNode.roads = w;
                        disconnectionNodes.Add(disconnectionNode);
                    }
                }
			}


            File.WriteAllText(fileName, JsonConvert.SerializeObject(disconnectionNodes));
        }

        // This method traverses the road networks and searches for disconnections in the road networked formed by the specified classes in the parameter list
        public void generateDisconnectionsDataBFS(List<string> roadClassification, string fileName)
        {
            List<JunctionNode> disconnectionNodes = new List<JunctionNode>();
            Dictionary<int, HashSet<int>> checker = new Dictionary<int, HashSet<int>>();
            //dictionary having Key= label value and Value = set of subtree for that label
            Dictionary<int, HashSet<JunctionNode>> LabelToSubtrees = new Dictionary<int, HashSet<JunctionNode>>();

            //queue for the children nodes
            Queue children;

            int currentLabel=0;

            //list of labels to be removed after bfs
            HashSet<int> LabelsToBeRemoved = new HashSet<int>();

            //loop thru all the nodes in junctionnodemap
            foreach (KeyValuePair<string, JunctionNode> node in JunctionNodeHashMap)
            {
                //if a junction node contains any of the required road type and is unlabeled
                if (node.Value.roadTypes.Intersect(roadClassification).Any() && node.Value.label == 0)
                {
                    //set of all the nodes of 1 type of label
                    HashSet<JunctionNode> subtree = new HashSet<JunctionNode>();

                    currentLabel++;
                    checker.Add(currentLabel, new HashSet<int>());
                    children = new Queue();

                    //initialize the parent with the current label
                    node.Value.label = currentLabel;
                    children.Enqueue(node.Value);
                    subtree.Add(node.Value);

                    while(children.Count != 0)
                    {
                        JunctionNode currentNode = (JunctionNode)children.Dequeue();
                        disconnectionNodes.Add(currentNode);

                        //look thru all the ways this particular node is present in
                        foreach(string way in NodeDictionary.GetValueOrDefault(currentNode.Id).ways)
                        {
                            Way wayObject = WayHashMap.GetValueOrDefault(way);

                            //if the road classification matches
                            if (roadClassification.Contains(wayObject.roadClass))
                            {
                                //get the label of the start node of that way
                                int label = JunctionNodeHashMap.GetValueOrDefault(wayObject.startNode.Id).label;

                                //if its label doesnt match the current label
                                if (label != currentLabel)
                                {
                                    //if the node is already labeled
                                    if(label != 0)
                                    {
                                        //append the entire subtree of that label to that of the currentlabel and update their label to currentlabel
                                        foreach (JunctionNode junctionNode in LabelToSubtrees.GetValueOrDefault(label))
                                        {
                                            junctionNode.label = currentLabel;
                                        }
                                        subtree.UnionWith(LabelToSubtrees.GetValueOrDefault(label));
                                        //flag that label
                                        LabelsToBeRemoved.Add(label);
                                        checker.GetValueOrDefault(label).Add(currentLabel);
                                    }
                                    //else it means that this node is unlabeled and add it to the children queue
                                    else
                                    {
                                        JunctionNodeHashMap.GetValueOrDefault(wayObject.startNode.Id).label = currentLabel;
                                        children.Enqueue(JunctionNodeHashMap.GetValueOrDefault(wayObject.startNode.Id));
                                        subtree.Add(JunctionNodeHashMap.GetValueOrDefault(wayObject.startNode.Id));

                                    }
                                    
                                }

                                //get the label of the end node of that way
                                label = JunctionNodeHashMap.GetValueOrDefault(wayObject.endNode.Id).label;
                                //if its label doesnt match the current label
                                if (label !=currentLabel)
                                {
                                    //if the node is already labeled
                                    if (label != 0)
                                    {
                                        //append the entire subtree of that label to that of the currentlabel and update their label to currentlabel
                                        foreach(JunctionNode junctionNode in LabelToSubtrees.GetValueOrDefault(label))
                                        {
                                            junctionNode.label = currentLabel;
                                        }
                                        subtree.UnionWith(LabelToSubtrees.GetValueOrDefault(label));
                                        //flag that label
                                        LabelsToBeRemoved.Add(label);
                                    }
                                    //else this node is unlabeled and add it to the children queue
                                    else
                                    {
                                        JunctionNodeHashMap.GetValueOrDefault(wayObject.endNode.Id).label = currentLabel;
                                        children.Enqueue(JunctionNodeHashMap.GetValueOrDefault(wayObject.endNode.Id));
                                        subtree.Add(JunctionNodeHashMap.GetValueOrDefault(wayObject.endNode.Id));

                                    }
                                }
                            }
                        }   
                    }
                    //add the subtree to the dictionary
                    LabelToSubtrees.Add(currentLabel, subtree);
                }
            }

            //remove all the labels which were merged with other labels
            foreach(int label in LabelsToBeRemoved)
            {
                LabelToSubtrees.Remove(label);
            }
           
            //write it to the file
            File.WriteAllText(fileName, JsonConvert.SerializeObject(LabelToSubtrees));
        }

        //This method generates a file to draw the road network of the specified classifications in the list parameter
        public void generateRoadNetwork(List<string> roadTypes,string fileName)
		{
            List<Way> ways = new List<Way>();

            //loop thru all the ways in the ways hashmap
            foreach(KeyValuePair<string,Way> way in WayHashMap)
			{
                //if the road type of the way is equal to one of the given road types
				if (roadTypes.Contains(way.Value.roadClass))
				{
                    //add it to the list
                    ways.Add(way.Value);
				}
			}

            //write it to the file
            File.WriteAllText(fileName, JsonConvert.SerializeObject(ways));
        }
    }
}
