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

        //distinct color codes for allocating them to different subgraphs
        static string[] ColourValues = new string[] {

"#63b598", "#ce7d78", "#ea9e70", "#a48a9e", "#c6e1e8", "#648177", "#0d5ac1",
"#f205e6", "#1c0365", "#14a9ad", "#4ca2f9", "#a4e43f", "#d298e2", "#6119d0",
"#d2737d", "#c0a43c", "#f2510e", "#651be6", "#79806e", "#61da5e", "#cd2f00",
"#9348af", "#01ac53", "#c5a4fb", "#996635", "#b11573", "#4bb473", "#75d89e",
"#2f3f94", "#2f7b99", "#da967d", "#34891f", "#b0d87b", "#ca4751", "#7e50a8",
"#c4d647", "#e0eeb8", "#11dec1", "#289812", "#566ca0", "#ffdbe1", "#2f1179",
"#935b6d", "#916988", "#513d98", "#aead3a", "#9e6d71", "#4b5bdc", "#0cd36d",
"#250662", "#cb5bea", "#228916", "#ac3e1b", "#df514a", "#539397", "#880977",
"#f697c1", "#ba96ce", "#679c9d", "#c6c42c", "#5d2c52", "#48b41b", "#e1cf3b",
"#5be4f0", "#57c4d8", "#a4d17a", "#225b8", "#be608b", "#96b00c", "#088baf",
"#f158bf", "#e145ba", "#ee91e3", "#05d371", "#5426e0", "#4834d0", "#802234",
"#6749e8", "#0971f0", "#8fb413", "#b2b4f0", "#c3c89d", "#c9a941", "#41d158",
"#fb21a3", "#51aed9", "#5bb32d", "#807fb", "#21538e", "#89d534", "#d36647",
"#7fb411", "#0023b8", "#3b8c2a", "#986b53", "#f50422", "#983f7a", "#ea24a3",
"#79352c", "#521250", "#c79ed2", "#d6dd92", "#e33e52", "#b2be57", "#fa06ec",
"#1bb699", "#6b2e5f", "#64820f", "#21538e", "#89d534", "#d36647",
"#7fb411", "#0023b8", "#3b8c2a", "#986b53", "#f50422", "#983f7a", "#ea24a3",
"#79352c", "#521250", "#c79ed2", "#d6dd92", "#e33e52", "#b2be57", "#fa06ec",
"#1bb699", "#6b2e5f", "#64820f", "#9cb64a", "#996c48", "#9ab9b7",
"#06e052", "#e3a481", "#0eb621", "#fc458e", "#b2db15", "#aa226d", "#792ed8",
"#73872a", "#520d3a", "#cefcb8", "#a5b3d9", "#7d1d85", "#c4fd57", "#f1ae16",
"#8fe22a", "#ef6e3c", "#243eeb", "#1dc18", "#dd93fd", "#3f8473", "#e7dbce",
"#421f79", "#7a3d93", "#635f6d", "#93f2d7", "#9b5c2a", "#15b9ee", "#0f5997",
"#409188", "#911e20", "#1350ce", "#10e5b1", "#fff4d7", "#cb2582", "#ce00be",
"#32d5d6", "#17232", "#608572", "#c79bc2", "#00f87c", "#77772a", "#6995ba",
"#fc6b57", "#f07815", "#8fd883", "#060e27", "#96e591", "#21d52e", "#d00043",
"#b47162", "#1ec227", "#4f0f6f", "#1d1d58", "#947002", "#bde052", "#e08c56",
"#28fcfd", "#bb09b", "#36486a", "#d02e29", "#1ae6db", "#3e464c", "#a84a8f",
"#911e7e", "#3f16d9", "#0f525f", "#ac7c0a", "#b4c086", "#c9d730", "#30cc49",
"#3d6751", "#fb4c03", "#640fc1", "#62c03e", "#d3493a", "#88aa0b", "#406df9",
"#615af0", "#4be47", "#2a3434", "#4a543f", "#79bca0", "#a8b8d4", "#00efd4",
"#7ad236", "#7260d8", "#1deaa7", "#06f43a", "#823c59", "#e3d94c", "#dc1c06",
"#f53b2a", "#b46238", "#2dfff6", "#a82b89", "#1a8011", "#436a9f", "#1a806a",
"#4cf09d", "#c188a2", "#67eb4b", "#b308d3", "#fc7e41", "#af3101", "#ff065",
"#71b1f4", "#a2f8a5", "#e23dd0", "#d3486d", "#00f7f9", "#474893", "#3cec35",
"#1c65cb", "#5d1d0c", "#2d7d2a", "#ff3420", "#5cdd87", "#a259a4", "#e4ac44",
"#1bede6", "#8798a4", "#d7790f", "#b2c24f", "#de73c2", "#d70a9c", "#25b67",
"#88e9b8", "#c2b0e2", "#86e98f", "#ae90e2", "#1a806b", "#436a9e", "#0ec0ff",
"#f812b3", "#b17fc9", "#8d6c2f", "#d3277a", "#2ca1ae", "#9685eb", "#8a96c6",
"#dba2e6", "#76fc1b", "#608fa4", "#20f6ba", "#07d7f6", "#dce77a", "#77ecca"
    };

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

            //Set of all disjoint graphs color coded 
            HashSet<Way> DisjointedSubTreeWays = new HashSet<Way>();

            //loop over all ways
            foreach (KeyValuePair<string, Way> way in WayHashMap)
            {
                //loop over all the disjointed hashset
                foreach (int labelNodes in LabelToSubtrees.Keys)
                {

                    //check which graph the way belongs to
                    if (LabelToSubtrees.GetValueOrDefault(labelNodes).Contains(JunctionNodeHashMap.GetValueOrDefault(way.Value.startNode.Id)))
                    {
                        //color code it accordingly
                        way.Value.colorCode = ColourValues[labelNodes];

                        //add it to the set which will be converted to a json file
                        DisjointedSubTreeWays.Add(way.Value);
                    }
                }
            }

            //write it to the file
            File.WriteAllText(fileName, JsonConvert.SerializeObject(DisjointedSubTreeWays.ToList()));
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
