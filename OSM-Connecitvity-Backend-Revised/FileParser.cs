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

        //Added in SUM
        //start
        //This dictionary contains the list of all ferry terminals
        Dictionary<string, Node> ferryTerminals = new Dictionary<string, Node>();

        //This dictionary contains the list of all nodes where Lng = 180 || -180. This was done to resolve 180 AM issue
        Dictionary<string, Node> nodesWithAM = new Dictionary<string, Node>();
        //end

        public FileParser(string OsmFilePath)
        {
            this.OsmFilePath = OsmFilePath;
        }

        public void createDataFiles()
        {
            //Added in SUM
            //start
            // This bool value will tell if the country has AM line passing through it. Right now it is three lands - FJ, Russia Antarctics
            // This value will be later set as a property for each country.
            bool hasAM = true;
            //The parameter will tell whether to populate nodeswithAM
            populateNodeDictionary(hasAM);
            //end
            XDocument doc = XDocument.Load(OsmFilePath);
            List<XElement> wayElements = doc.Descendants("way").ToList();

            foreach (XElement el in wayElements)
            {
                Way way = new Way();

                // Populating way object 
                way.Id = el.FirstAttribute.Value;


                //Added in SUM: the ferry tags
                //start
                //If no highway is found, we inspect route tags to see if this is a ferry way and use the roadClass attribute to set whether way is a ferry route
                List<XElement> inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "highway").ToList();
                string ferry = null;
                if (inspectedTag.Count == 0)
                {
                    List<XElement> inspectedRouteTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "route").ToList();
                    ferry = inspectedRouteTag.Count == 0 ? "" : ((XElement)inspectedRouteTag.First()).LastAttribute.Value;
                    if (!ferry.Equals("ferry"))
                        ferry = "";
                }
                way.roadClass = inspectedTag.Count == 0 ? ferry : ((XElement)inspectedTag.First()).LastAttribute.Value;
                //end


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
                if (junctionNodeDictionary.ContainsKey(startNode.Id))
                {
                    firstJunction = junctionNodeDictionary[startNode.Id];
                    firstJunction.roadTypes.Add(way.roadClass);
                    firstJunction.wayToNodeMap[way.Id] = endNode.Id;
                    junctionNodeDictionary[startNode.Id] = firstJunction;
                }
                else
                {
                    //Modified in SUM
                    firstJunction = new JunctionNode(startNode.Id, new Dictionary<string, string>() { { way.Id, endNode.Id } }, new List<string>() { { way.roadClass } }, startNode.Lat, startNode.Lng);
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
                    //Modified in SUM
                    lastJunction = new JunctionNode(endNode.Id, new Dictionary<string, string>() { { way.Id, startNode.Id } }, new List<string>() { { way.roadClass } }, endNode.Lat, endNode.Lng);
                    junctionNodeDictionary.Add(endNode.Id, lastJunction);
                }

                wayDictionary.Add(way.Id, way);
                //Console.WriteLine(way.Id);

            }

            //Added in SUM
            //start
            if (hasAM)
                generateAMWays();
            find_T_IntersectionWays();


            extractRelationsFromOSM();
            //end

            // Create the way data file
            File.WriteAllText("ways.json", JsonConvert.SerializeObject(wayDictionary));
            Console.WriteLine("Successfully created ways.json file");

            // Create the junctionNodes data file
            File.WriteAllText("junctionNodes.json", JsonConvert.SerializeObject(junctionNodeDictionary));
            Console.WriteLine("Successfully created junctionNodes.json file");

            // Create the node data file
            File.WriteAllText("NodeDictionary.json", JsonConvert.SerializeObject(NodeDictionary));
            Console.WriteLine("Successfully created NodeDictionary.json file");

            //Added in SUM
            // Create the node data file
            File.WriteAllText("FJ_FerryTerminals.json", JsonConvert.SerializeObject(ferryTerminals));
            Console.WriteLine("Successfully created Ferryterminals.json file");

        }

        public void populateNodeDictionary(bool hasAM)
        {
            XDocument doc = XDocument.Load(OsmFilePath);
            List<XElement> elements = doc.Descendants("node").ToList();
            foreach (XElement el in elements)
            {
                String nodeid = el.Attribute("id").Value;
                //Console.WriteLine(nodeid);
                Node node = new Node(nodeid, float.Parse(el.Attribute("lat").Value), float.Parse(el.Attribute("lon").Value));

                //Added in SUM
                //start
                //Fetches the ferry terminals
                List<XElement> inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "amenity").ToList();
                String ferry_terminal = inspectedTag.Count == 0 ? "" : ((XElement)inspectedTag.First()).LastAttribute.Value;
                if (ferry_terminal.Equals("ferry_terminal"))
                    ferryTerminals.Add(nodeid, node);

                NodeDictionary.Add(nodeid, node);
                //Collects nodes that lie on 180 AM line
                if (hasAM && (float.Parse(el.Attribute("lon").Value).Equals(180)) || (float.Parse(el.Attribute("lon").Value).Equals(-180)))
                {
                    nodesWithAM.Add(nodeid, node);
                }
                //end
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
                            if (flag == -1)
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


        //Added in SUM
        //start
        //For nodes that lie on 180 AM, if there are complementary nodes
        // Eg N1 (16.78,180) and N2(16.78, -180). N1 and N2 are physically same but cartographically differnet.
        // We add a hypothetical way between these two nodes, to make them reachable as one.
        // Currently the hypothetical way is bi-directional independant of the directionality of the N1 and N2
        public void generateAMWays()
        {

            //cheked Nodes to see if a way was created for the complementary node
            HashSet<string> checkedNodeIds = new HashSet<string>();
            //Naming for multiple ways
            int c = 97;
            //loop thru all the AM nodes
            try
            {
                foreach (Node currentNode in nodesWithAM.Values)
                {
                    //Only if current node or it's complementary node were not inspected
                    if (!checkedNodeIds.Contains(currentNode.Id))
                    {
                        var complementNodes = nodesWithAM.Values.GroupBy(x => x.Lat == currentNode.Lat).
                            Where(x => x.Count() > 1).Where(g => g.Key == true);

                        if (complementNodes.Count() > 0)
                        {
                            //if currentNode is N1, we find N2
                            Node complementNode = complementNodes.ElementAt(0).Where(n => n.Id != (currentNode.Id)).ToList()[0];

                            checkedNodeIds.Add(complementNode.Id);

                            //Creating a hyopthetical way
                            Way hypoAMWay = new Way();
                            hypoAMWay.Id = "AM-" + (char)(c++);
                            hypoAMWay.startNode = currentNode;
                            hypoAMWay.endNode = complementNode;
                            //hypoAMWay.maxSpeed = 0;
                            //hypoAMWay.oneWay = wayDictionary.GetValueOrDefault(way).oneWay;
                            hypoAMWay.roadClass = "AM";
                            hypoAMWay.name = "HypotheticalWayForAM";
                            hypoAMWay.nodes.Add(currentNode);
                            hypoAMWay.nodes.Add(complementNode);

                            //Update the way dictionary
                            wayDictionary.Add(hypoAMWay.Id, hypoAMWay);

                            //Update data for N1 and N2
                            currentNode.ways.Add(hypoAMWay.Id);
                            currentNode.roadClasses.Add(hypoAMWay.roadClass);
                            complementNode.ways.Add(hypoAMWay.Id);
                            complementNode.roadClasses.Add(hypoAMWay.roadClass);


                            //update the start junction node value
                            JunctionNode junctionNode = junctionNodeDictionary.GetValueOrDefault(hypoAMWay.startNode.Id);

                            if (junctionNode != null)
                            {
                                junctionNode.roadTypes.Add(hypoAMWay.roadClass);

                                junctionNode.wayToNodeMap[hypoAMWay.Id] = hypoAMWay.endNode.Id;
                                junctionNodeDictionary[hypoAMWay.startNode.Id] = junctionNode;
                            }
                            else
                            {
                                junctionNode = new JunctionNode();
                                junctionNode.Id = hypoAMWay.startNode.Id;
                                junctionNode.Lat = hypoAMWay.startNode.Lat;
                                junctionNode.Lng = hypoAMWay.startNode.Lng;
                                junctionNode.roadTypes.Add(hypoAMWay.roadClass);
                                junctionNode.wayToNodeMap.Add(hypoAMWay.Id, hypoAMWay.endNode.Id);
                                junctionNodeDictionary.Add(junctionNode.Id, junctionNode);



                            }

                            //update the end junction node value
                            junctionNode = junctionNodeDictionary.GetValueOrDefault(hypoAMWay.endNode.Id);

                            if (junctionNode != null)
                            {

                                junctionNode.roadTypes.Add(hypoAMWay.roadClass);

                                junctionNode.wayToNodeMap[hypoAMWay.Id] = hypoAMWay.startNode.Id;
                                junctionNodeDictionary[hypoAMWay.endNode.Id] = junctionNode;
                            }
                            else
                            {
                                junctionNode = new JunctionNode();
                                junctionNode.Id = hypoAMWay.endNode.Id;
                                junctionNode.Lat = hypoAMWay.endNode.Lat;
                                junctionNode.Lng = hypoAMWay.endNode.Lng;
                                junctionNode.roadTypes.Add(hypoAMWay.roadClass);
                                junctionNode.wayToNodeMap.Add(hypoAMWay.Id, hypoAMWay.startNode.Id);
                                junctionNodeDictionary.Add(junctionNode.Id, junctionNode);
                            }




                        }
                        checkedNodeIds.Add(currentNode.Id);

                        //There is a case where there is no pair for the node. We do not connect hypothetical way. We live it as is.

                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occured" + ex.StackTrace + "\n" + ex.Message);
            }



        }



        //Extracts relations from relations tag and updates the junction Nodes
        // We currently consider if a relations has [From,Via,To] or [From, Via] tag combinations. We leave others out
        // We consider restrictions with "no_*" tags only.
        public void extractRelationsFromOSM()
        {
            try
            {
                XDocument doc = XDocument.Load(OsmFilePath);
                List<XElement> elements = doc.Descendants("relation").ToList();
                foreach (XElement el in elements)
                {

                    string relationId = el.FirstAttribute.Value;
                    List<string> restrictions = new List<string>();
                    List<KeyValuePair<string, string>> memberWays = new List<KeyValuePair<string, string>>();
                    Dictionary<string, string> memberNodes = new Dictionary<string, string>();

                    List<string> To_wayIdToAddInJunctionNode = new List<string>();
                    List<string> From_wayIdToAddInJunctionNode = new List<string>();


                    //Defines the via Node 
                    string junctionNodeId = null;
                    //Define if the restriction is a no restriction
                    string restriction = null;

                    // Whether this relations satisfies the required conditions - [F,V,T] or [F,T] or "No"
                    bool addThisRelation = false;
                    //IF the relation has only [F,T], we need to find a Via point
                    bool findTo = false;

                    //We find the Number of froms, Tos, Vias for each relation, This is used for analysis 
                    int numFroms = el.Descendants("member").Where(x => (string)x.Attribute("role") == "from").ToList().Count;
                    int numTos = el.Descendants("member").Where(x => (string)x.Attribute("role") == "to").ToList().Count;
                    int numVias = el.Descendants("member").Where(x => (string)x.Attribute("role") == "via").ToList().Count;


                    /*
                    if ((numFroms > 0 && numTos > 0) || (numFroms > 0 && numTos > 0 && numVias > 0))
                    {
                        addThisRelation = true;
                        Console.WriteLine("Relation Id:" + relationId + ":will be added");
                    }
                    */


                    int numDistinctFroms = (el.Descendants("member").Where(x => (string)x.Attribute("role") == "from").ToList().Count) == 0 ? 0 : 1;
                    int numDistinctTos = (el.Descendants("member").Where(x => (string)x.Attribute("role") == "to").ToList().Count) == 0 ? 0 : 1;
                    int numDistinctVias = (el.Descendants("member").Where(x => (string)x.Attribute("role") == "via").ToList().Count) == 0 ? 0 : 1;

                    //Consider F-V-T only
                    if ((numDistinctFroms + numDistinctTos + numDistinctVias) == 3)
                    {
                        addThisRelation = true;
                        //Console.WriteLine("Relation Id:" + relationId + ":will be added" + ":" + numDistinctFroms + ":" + numDistinctVias + ":" + numDistinctTos);

                    }
                    //Consider F-T also
                    if ((numDistinctFroms + numDistinctTos + numDistinctVias) == 2 && numDistinctVias == 0)
                    {
                        addThisRelation = true;
                        findTo = true;
                    }


                    if (addThisRelation)
                    {


                        List<XElement> restrictionTags = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "restriction").ToList();
                        if (restrictionTags.Count != 0)
                        {
                            foreach (XElement restrElement in restrictionTags)
                            {
                                string thisRestriction = restrElement.Attribute("v").Value.ToString();
                                //if (numDistinctVias > 0)
                                //    restriction = thisRestriction;
                                restriction = thisRestriction;
                                restrictions.Add(thisRestriction);
                            }

                        }

                        foreach (XElement memElement in el.Descendants("member").ToList())
                        {

                            string memberType = memElement.Attribute("type").Value.ToString();
                            if (memberType.Equals("way"))
                            {


                                //If the way exists, add it with its role
                                string thisWayId = memElement.Attribute("ref").Value.ToString();
                                string roleOfWay = memElement.Attribute("role").Value.ToString();

                                if (this.wayDictionary.Keys.Any(k => k.Contains(thisWayId)))
                                {
                                    //ways created when Finding T intersections will be handled here
                                    List<string> matchingKeys = this.wayDictionary.Keys.Where(x => x.Contains(thisWayId)).ToList();
                                    if (matchingKeys.Count == 1)
                                    {
                                        if (roleOfWay.Contains("to"))// && (numDistinctVias > 0 || findTo == true))
                                        {
                                            To_wayIdToAddInJunctionNode.Add(thisWayId);
                                        }
                                        if (roleOfWay.Contains("from"))// && (numDistinctVias > 0 || findTo == true))
                                        {
                                            From_wayIdToAddInJunctionNode.Add(thisWayId);
                                        }
                                        memberWays.Add(new KeyValuePair<string, string>(thisWayId, roleOfWay));

                                    }
                                    else
                                    {
                                        if (roleOfWay.Contains("to"))
                                        {
                                            To_wayIdToAddInJunctionNode.Add(matchingKeys.First());
                                            memberWays.Add(new KeyValuePair<string, string>(matchingKeys.First(), roleOfWay));
                                        }
                                        else if (roleOfWay.Contains("from"))
                                        {
                                            From_wayIdToAddInJunctionNode.Add(matchingKeys.Last());
                                            memberWays.Add(new KeyValuePair<string, string>(matchingKeys.Last(), roleOfWay));
                                        }
                                    }

                                }
                            }
                            else if (memberType.Equals("node"))
                            {

                                string thisNodeId = memElement.Attribute("ref").Value.ToString();
                                string roleOfNode = memElement.Attribute("role").Value.ToString();
                                if (this.NodeDictionary.ContainsKey(thisNodeId))
                                {
                                    if (roleOfNode.Contains("via") && numDistinctVias > 0)
                                    {

                                        junctionNodeId = thisNodeId;


                                    }
                                    memberNodes.Add(thisNodeId, roleOfNode);

                                }



                            }



                        }
                        //if it is a F,T relation
                        if (findTo)
                        {
                            if (From_wayIdToAddInJunctionNode.Count > 0 && To_wayIdToAddInJunctionNode.Count > 0)
                            {
                                string endNode = this.wayDictionary.GetValueOrDefault(From_wayIdToAddInJunctionNode[0]).endNode.Id;
                                string startNode = this.wayDictionary.GetValueOrDefault(To_wayIdToAddInJunctionNode[0]).startNode.Id;
                                if (endNode.Equals(startNode))
                                {
                                    junctionNodeId = endNode;
                                    memberNodes.Add(junctionNodeId, "via");

                                }


                            }
                        }



                        if (junctionNodeId != null && (restriction != null && restriction.StartsWith("no_")))
                        {



                            JunctionNode viaJnNode = this.junctionNodeDictionary.GetValueOrDefault(junctionNodeId);
                            if (viaJnNode != null)
                            {
                                viaJnNode.restrictions[relationId] = "via";
                                foreach (string wayId in To_wayIdToAddInJunctionNode)
                                {
                                    if (junctionNodeId.Equals(this.wayDictionary.GetValueOrDefault(wayId).startNode.Id))
                                    {
                                        this.junctionNodeDictionary.GetValueOrDefault(this.wayDictionary.GetValueOrDefault(wayId).endNode.Id)
                                            .restrictions[relationId] = "to";
                                    }

                                }
                                foreach (string wayId in From_wayIdToAddInJunctionNode)
                                {
                                    if (junctionNodeId.Equals(this.wayDictionary.GetValueOrDefault(wayId).endNode.Id))
                                    {
                                        this.junctionNodeDictionary.GetValueOrDefault(this.wayDictionary.GetValueOrDefault(wayId).startNode.Id)
                                            .restrictions[relationId] = "from";
                                    }
                                }
                            }



                        }

                        //Store data if anlaysis required
                        /*
                        mainRelationDictionary.Add(relationId,
                                new Relation(relationId, restrictions, memberWays, memberNodes));

                        //Fetches F-T relations
                        if ((numDistinctFroms > 0 && numDistinctTos > 0 && numDistinctVias == 0))
                        {
                            FTRelationDictionary.Add(relationId,
                                                        new Relation(relationId, restrictions, memberWays, memberNodes));
                        }*/
                    }

                }




                /*File.WriteAllText("NZ_Relations.json", JsonConvert.SerializeObject(mainRelationDictionary));
                Console.WriteLine("Successfully written relations in json file");

                File.WriteAllText("NZ_FT_Relations.json", JsonConvert.SerializeObject(FTRelationDictionary));
                Console.WriteLine("Successfully written F-T relations in json file"); */
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occured" + ex.StackTrace + "\n" + ex.Message);
            }
        }
    }
}
