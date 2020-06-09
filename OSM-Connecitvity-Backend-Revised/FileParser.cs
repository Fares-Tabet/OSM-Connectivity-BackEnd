using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace OSM_Connecitvity_Backend_Revised
{
    class FileParser
    {
        //path of the main xml file
        public string OsmFilePath;

        //This dictionary contains the all the node coordinates and will be serialized into json and added to the DataFile folder
        Dictionary<string, Node> NodeDictionary = new Dictionary<string, Node>();

        //This dictionary contains the way objects and will be serialized into json and added to the DataFile folder
        Dictionary<string, Way> wayDictionary = new Dictionary<string, Way>();

        //This dictionary contains the intersected way objects and will be serialized into json and added to the DataFile folder
        Dictionary<string, Way> intersectionWaysDictionary = new Dictionary<string, Way>();

        //This dictionary contains the junctionNOde objects and will be serialized into json and added to the DataFile folder
        Dictionary<string, JunctionNode> junctionNodeDictionary = new Dictionary<string, JunctionNode>();

        public FileParser(string OsmFilePath)
        {
            this.OsmFilePath = OsmFilePath;
        }

        public void createDataFiles()
        {
            populateNodeDictionary();
            XDocument doc = XDocument.Load(OsmFilePath);
            List<XElement> wayElements = doc.Descendants("way").ToList();
             
            foreach (XElement el in wayElements)
            {
                Way way = new Way();

                // Populating way object 
                way.Id = el.FirstAttribute.Value;

                

                List<XElement> inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "highway").ToList();
                way.roadClass = inspectedTag.Count == 0 ? "" : ((XElement)inspectedTag.First()).LastAttribute.Value;

                inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "name").ToList();
                way.name = inspectedTag.Count == 0 ? "" : ((XElement)inspectedTag.First()).LastAttribute.Value;

                inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "maxspeed").ToList();
                way.maxSpeed = inspectedTag.Count == 0 ? "" : ((XElement)inspectedTag.First()).LastAttribute.Value;

                inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "oneway").ToList();
                way.oneWay = inspectedTag.Count == 0 ? "" : ((XElement)inspectedTag.First()).LastAttribute.Value;

                // create a temp node = current way node but without any children                
                XElement temp = new XElement(el);
                temp.RemoveAll();

                // create the list of nodes that will be added to the way object
                List<Node> nodeList = new List<Node>();

                // loop though the nodes of each way and populate the way object
                foreach (XElement nd in el.Descendants("nd").ToList())
                {
                    Node node = NodeDictionary.GetValueOrDefault(nd.FirstAttribute.Value);
                    nodeList.Add(node);
                    NodeDictionary.GetValueOrDefault(nd.FirstAttribute.Value).roadClasses.Add(way.roadClass);
                    NodeDictionary.GetValueOrDefault(nd.FirstAttribute.Value).ways.Add(way.Id);
                 
                }

                way.nodes = nodeList;

                // popoulate the start and end node field of the way object
                XElement endPoint1 = el.Descendants("nd").ToList().First();
                Node startNode = NodeDictionary.GetValueOrDefault(endPoint1.FirstAttribute.Value);
                way.startNode = startNode;

                XElement endPoint2 = el.Descendants("nd").ToList().Last();
                Node endNode = NodeDictionary.GetValueOrDefault(endPoint2.FirstAttribute.Value);
                way.endNode = endNode;

                // populate the junctionNodeDictionary with the first and last node of each way
                JunctionNode firstJunction;
                if(junctionNodeDictionary.ContainsKey(startNode.Id))
                {
                    firstJunction = junctionNodeDictionary[startNode.Id];
                    firstJunction.roadTypes.Add(way.roadClass);
                    firstJunction.wayToNodeMap[way.Id] = endNode.Id; 
                    junctionNodeDictionary[startNode.Id] = firstJunction;
                }
                else
                {
                    firstJunction = new JunctionNode(startNode.Id, new Dictionary<string, string>() { { way.Id, endNode.Id } }, new List<string>() { { way.roadClass } }, endNode.Lat, endNode.Lng);
                    junctionNodeDictionary.Add(startNode.Id, firstJunction);
                }

                JunctionNode lastJunction;
                if (junctionNodeDictionary.ContainsKey(endNode.Id))
                {
                    lastJunction = junctionNodeDictionary[endNode.Id];
                    lastJunction.roadTypes.Add(way.roadClass);
                    lastJunction.wayToNodeMap[way.Id] = startNode.Id;   
                    junctionNodeDictionary[endNode.Id] = lastJunction;
                }
                else
                {
                    lastJunction = new JunctionNode(endNode.Id, new Dictionary<string, string>() { { way.Id, startNode.Id } }, new List<string>() { { way.roadClass } }, startNode.Lat, startNode.Lng);
                    junctionNodeDictionary.Add(endNode.Id, lastJunction);
                }

                wayDictionary.Add(way.Id,way);
                //Console.WriteLine(way.Id);

            }

            
            
            // Create the way data file
            File.WriteAllText("ways.json", JsonConvert.SerializeObject(wayDictionary));
            Console.WriteLine("Successfully created ways.json file");

            // Create the junctionNodes data file
            File.WriteAllText("junctionNodes.json", JsonConvert.SerializeObject(junctionNodeDictionary));
            Console.WriteLine("Successfully created junctionNodes.json file");

            // Create the node data file
            File.WriteAllText("NodeDictionary.json", JsonConvert.SerializeObject(NodeDictionary));
            Console.WriteLine("Successfully created NodeDictionary.json file");

        }

        public void populateNodeDictionary()
        {
            XDocument doc = XDocument.Load(OsmFilePath);
            List<XElement> elements = doc.Descendants("node").ToList();
            foreach (XElement el in elements)
            {
                String nodeid = el.Attribute("id").Value;
                //Console.WriteLine(nodeid);
                Node node = new Node(nodeid, float.Parse(el.Attribute("lat").Value), float.Parse(el.Attribute("lon").Value));
                NodeDictionary.Add(nodeid, node);
            }
        }

        //method used to find all ways which involve T intersection
        public void find_T_IntersectionWays()
        {
            //list of ways which are present in a T interestion and need to be dissected
            HashSet<string> waysToBeRemoved = new HashSet<string>();

            //list of dissected ways with there new wayids
            HashSet<Way> addedWays = new HashSet<Way>();

            //loop thru all the junction nodes
            foreach (JunctionNode jnode in junctionNodeDictionary.Values)
            {
                //if no. of ways a junction node is present in is not equal to the number of ways it is present in the node dictionary
                if (jnode.wayToNodeMap.Keys.Count() != NodeDictionary.GetValueOrDefault(jnode.Id).ways.Count())
                {
                    //list of ways a node is present in ( (from the node dictionary) - (junctionnode waytohashmap list))
                    List<string> ways = NodeDictionary.GetValueOrDefault(jnode.Id).ways.Except(jnode.wayToNodeMap.Keys).ToList();
                    Way wayToBeAdded;
                    int c;
                    int flag = 1;
                    foreach (string way in ways)
                    {
                        //if a way is already dissected then dont do the below steps
                        if (waysToBeRemoved.Contains(way))
                            continue;

                        //else add the way to the waystoberemoved list
                        waysToBeRemoved.Add(way);

                        //initialize a new way, which will get added to the ways hashmap
                        wayToBeAdded = new Way();
                        flag = 1;

                        //to append "a" to the wayid (96 = ascii value)
                        c = 96;

                        foreach (Node node in wayDictionary.GetValueOrDefault(way).nodes)
                        {

                            //if this is the first dissection of the way (i.e. wayid + "a")
                            if (flag == 1)
                            {
                                //populate the way data into the new way object
                                wayToBeAdded.Id = way + (char)(c + 1);
                                wayToBeAdded.startNode = node;
                                wayToBeAdded.maxSpeed = wayDictionary.GetValueOrDefault(way).maxSpeed;
                                wayToBeAdded.oneWay = wayDictionary.GetValueOrDefault(way).oneWay;
                                wayToBeAdded.roadClass = wayDictionary.GetValueOrDefault(way).roadClass;
                                wayToBeAdded.name = wayDictionary.GetValueOrDefault(way).name;
                                wayToBeAdded.nodes.Add(node);
                                flag = 0;
                                continue;
                            }
                            //if the number of roadclasses is 1, it implies that it is the end node or a start node, hence change the flag to end this way object and initialize a new one
                            if (node.roadClasses.Count > 1 || wayDictionary.GetValueOrDefault(way).endNode.Id.Equals(node.Id))
                            {
                                flag = -1;
                            }
                            //add the node to the geometry of the object
                            else
                            {
                                wayToBeAdded.nodes.Add(node);
                                flag = 0;
                            }
                            //end a way object and start a new object
                            if(flag == -1)
                            {
                                //add the way object to the way hashmap
                                wayToBeAdded.endNode = node;
                                wayToBeAdded.nodes.Add(node);
                                wayDictionary.Add(wayToBeAdded.Id, wayToBeAdded);
                               
                                addedWays.Add(wayToBeAdded);

                                //add the new way to be added to the intersection ways dictionary
                                intersectionWaysDictionary.Add(wayToBeAdded.Id, wayDictionary.GetValueOrDefault(way));

                                //initialize a new way object and add the way data to this way
                                c = c + 1;
                                wayToBeAdded = new Way();
                                wayToBeAdded.Id = way + (char)(c + 1);
                                wayToBeAdded.startNode = node;
                                wayToBeAdded.maxSpeed = wayDictionary.GetValueOrDefault(way).maxSpeed;
                                wayToBeAdded.oneWay = wayDictionary.GetValueOrDefault(way).oneWay;
                                wayToBeAdded.roadClass = wayDictionary.GetValueOrDefault(way).roadClass;
                                wayToBeAdded.name = wayDictionary.GetValueOrDefault(way).name;
                                wayToBeAdded.nodes.Add(node);

                                //change the flag so that it doesnt enter the same loop until the flag is changed again
                                flag = 0;
                            }
                        }
                    }
                }
            }

            //update the way ids in the junction node hashmap
            foreach (Way way in addedWays)
            {
                //update the start junction node value
                JunctionNode junctionNode = junctionNodeDictionary.GetValueOrDefault(way.startNode.Id);
               
                if (junctionNode != null)
                {
                    if (junctionNode.wayToNodeMap.ContainsKey(way.Id.Substring(0, way.Id.Length - 1)))
                    {
                        junctionNode.roadTypes.Remove(wayDictionary.GetValueOrDefault(way.Id).roadClass);
                    }
                    junctionNode.roadTypes.Add(way.roadClass);
                    junctionNode.wayToNodeMap.Remove(way.Id.Substring(0, way.Id.Length - 1));
                    junctionNode.wayToNodeMap[way.Id] = way.endNode.Id;
                    junctionNodeDictionary[way.startNode.Id] = junctionNode;
                }
                else
                {
                    junctionNode = new JunctionNode();
                    junctionNode.Id = way.startNode.Id;
                    junctionNode.Lat = way.startNode.Lat;
                    junctionNode.Lng = way.startNode.Lng;
                    junctionNode.roadTypes.Add(way.roadClass);
                    junctionNode.wayToNodeMap.Add(way.Id, way.endNode.Id);
                    junctionNodeDictionary.Add(junctionNode.Id, junctionNode);



                }



                //update the end junction node value
                junctionNode = junctionNodeDictionary.GetValueOrDefault(way.endNode.Id);
                
                if (junctionNode != null)
                {
                    if (junctionNode.wayToNodeMap.ContainsKey(way.Id.Substring(0, way.Id.Length - 1)))
                    {
                        junctionNode.roadTypes.Remove(wayDictionary.GetValueOrDefault(way.Id).roadClass);
                    }
                    junctionNode.roadTypes.Add(way.roadClass);
                    junctionNode.wayToNodeMap.Remove(way.Id.Substring(0, way.Id.Length - 1));
                    junctionNode.wayToNodeMap[way.Id] = way.startNode.Id;
                    junctionNodeDictionary[way.endNode.Id] = junctionNode;
                }
                else
                {
                    junctionNode = new JunctionNode();
                    junctionNode.Id = way.endNode.Id;
                    junctionNode.Lat = way.endNode.Lat;
                    junctionNode.Lng = way.endNode.Lng;
                    junctionNode.roadTypes.Add(way.roadClass);
                    junctionNode.wayToNodeMap.Add(way.Id, way.startNode.Id);
                    junctionNodeDictionary.Add(junctionNode.Id, junctionNode);
                }



                //make changes in the roadclasses and way list of the nodes in the node dictionary
                foreach (Node node in way.nodes)
                {
                    string wayid = way.Id.Substring(0, way.Id.Length - 1);
                    if (node.ways.Contains(wayid) || node.ways.Contains(way.Id))
                    {
                        node.roadClasses.Remove(wayDictionary.GetValueOrDefault(wayid).roadClass);
                        
                    }
                    node.roadClasses.Add(way.roadClass);
                    node.ways.Remove(wayid);
                    node.ways.Remove(way.Id);
                    node.ways.Add(way.Id);
                }
            }

            //remove the T intersetion way from the way hashmap
            foreach (string way in waysToBeRemoved)
            {
                wayDictionary.Remove(way);
            }

        }
    }
}
