using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

//TODO
/* Do something about the color thing
 * Merge the two BFS helpers to have only 1 with a boolean to say if we want to call it without target graph or not
 *
*/


namespace OSM_Connecitvity_Backend_Revised
{
    public class FileProcessor
    {
        Dictionary<string, Node> NodeDictionary;
        Dictionary<string, Way> WayHashMap;
        Dictionary<string, JunctionNode> JunctionNodeHashMap;

        //Set of all disjoint graphs color coded 
        HashSet<Way> DisjointedSubTreeWays;

        public Dictionary<string, int> nodeToVertex { get; set; }
        //distinct color codes for allocating them to different subgraphs
        static string[] ColorValues = new string[] {

"#63b598", "#ce7d78", "#ea9e70", "#a48a9e", "#c6e1e8", "#648177", "#0d5ac1", "#f205e6", "#1c0365", "#14a9ad", "#4ca2f9", "#a4e43f", "#d298e2", "#6119d0",
"#d2737d", "#c0a43c", "#f2510e", "#651be6", "#79806e", "#61da5e", "#cd2f00","#9348af", "#01ac53", "#c5a4fb", "#996635", "#b11573", "#4bb473", "#75d89e",
"#2f3f94", "#2f7b99", "#da967d", "#34891f", "#b0d87b", "#ca4751", "#7e50a8","#c4d647", "#e0eeb8", "#11dec1", "#289812", "#566ca0", "#ffdbe1", "#2f1179",
"#935b6d", "#916988", "#513d98", "#aead3a", "#9e6d71", "#4b5bdc", "#0cd36d","#250662", "#cb5bea", "#228916", "#ac3e1b", "#df514a", "#539397", "#880977",
"#f697c1", "#ba96ce", "#679c9d", "#c6c42c", "#5d2c52", "#48b41b", "#e1cf3b","#5be4f0", "#57c4d8", "#a4d17a", "#225b8", "#be608b", "#96b00c", "#088baf",
"#f158bf", "#e145ba", "#ee91e3", "#05d371", "#5426e0", "#4834d0", "#802234","#6749e8", "#0971f0", "#8fb413", "#b2b4f0", "#c3c89d", "#c9a941", "#41d158",
"#fb21a3", "#51aed9", "#5bb32d", "#807fb", "#21538e", "#89d534", "#d36647","#7fb411", "#0023b8", "#3b8c2a", "#986b53", "#f50422", "#983f7a", "#ea24a3",
"#79352c", "#521250", "#c79ed2", "#d6dd92", "#e33e52", "#b2be57", "#fa06ec","#1bb699", "#6b2e5f", "#64820f", "#21538e", "#89d534", "#d36647","#7fb411",
"#0023b8", "#3b8c2a", "#986b53", "#f50422", "#983f7a", "#ea24a3","#79352c", "#521250", "#c79ed2", "#d6dd92", "#e33e52", "#b2be57", "#fa06ec",
"#1bb699", "#6b2e5f", "#64820f", "#9cb64a", "#996c48", "#9ab9b7","#06e052", "#e3a481", "#0eb621", "#fc458e", "#b2db15", "#aa226d", "#792ed8",
"#73872a", "#520d3a", "#cefcb8", "#a5b3d9", "#7d1d85", "#c4fd57", "#f1ae16","#8fe22a", "#ef6e3c", "#243eeb", "#1dc18", "#dd93fd", "#3f8473", "#e7dbce",
"#421f79", "#7a3d93", "#635f6d", "#93f2d7", "#9b5c2a", "#15b9ee", "#0f5997","#409188", "#911e20", "#1350ce", "#10e5b1", "#fff4d7", "#cb2582", "#ce00be",
"#32d5d6", "#17232", "#608572", "#c79bc2", "#00f87c", "#77772a", "#6995ba","#fc6b57", "#f07815", "#8fd883", "#060e27", "#96e591", "#21d52e", "#d00043",
"#b47162", "#1ec227", "#4f0f6f", "#1d1d58", "#947002", "#bde052", "#e08c56","#28fcfd", "#bb09b", "#36486a", "#d02e29", "#1ae6db", "#3e464c", "#a84a8f",
"#911e7e", "#3f16d9", "#0f525f", "#ac7c0a", "#b4c086", "#c9d730", "#30cc49","#3d6751", "#fb4c03", "#640fc1", "#62c03e", "#d3493a", "#88aa0b", "#406df9",
"#615af0", "#4be47", "#2a3434", "#4a543f", "#79bca0", "#a8b8d4", "#00efd4","#7ad236", "#7260d8", "#1deaa7", "#06f43a", "#823c59", "#e3d94c", "#dc1c06",
"#f53b2a", "#b46238", "#2dfff6", "#a82b89", "#1a8011", "#436a9f", "#1a806a","#4cf09d", "#c188a2", "#67eb4b", "#b308d3", "#fc7e41", "#af3101", "#ff065",
"#71b1f4", "#a2f8a5", "#e23dd0", "#d3486d", "#00f7f9", "#474893", "#3cec35","#1c65cb", "#5d1d0c", "#2d7d2a", "#ff3420", "#5cdd87", "#a259a4", "#e4ac44",
"#1bede6", "#8798a4", "#d7790f", "#b2c24f", "#de73c2", "#d70a9c", "#25b67","#88e9b8", "#c2b0e2", "#86e98f", "#ae90e2", "#1a806b", "#436a9e", "#0ec0ff",
"#f812b3", "#b17fc9", "#8d6c2f", "#d3277a", "#2ca1ae", "#9685eb", "#8a96c6","#dba2e6", "#76fc1b", "#608fa4", "#20f6ba", "#07d7f6", "#dce77a", "#77ecca",
"#fb21a3", "#51aed9", "#5bb32d", "#807fb", "#21538e", "#89d534", "#d36647","#7fb411", "#0023b8", "#3b8c2a", "#986b53", "#f50422", "#983f7a", "#ea24a3",
"#79352c", "#521250", "#c79ed2", "#d6dd92", "#e33e52", "#b2be57", "#fa06ec","#1bb699", "#6b2e5f", "#64820f", "#21538e", "#89d534", "#d36647","#7fb411",
"#0023b8", "#3b8c2a", "#986b53", "#f50422", "#983f7a", "#ea24a3","#79352c", "#521250", "#c79ed2", "#d6dd92", "#e33e52", "#b2be57", "#fa06ec",
"#1bb699", "#6b2e5f", "#64820f", "#9cb64a", "#996c48", "#9ab9b7","#06e052", "#e3a481", "#0eb621", "#fc458e", "#b2db15", "#aa226d", "#792ed8",
"#73872a", "#520d3a", "#cefcb8", "#a5b3d9", "#7d1d85", "#c4fd57", "#f1ae16","#8fe22a", "#ef6e3c", "#243eeb", "#1dc18", "#dd93fd", "#3f8473", "#e7dbce",
"#421f79", "#7a3d93", "#635f6d", "#93f2d7", "#9b5c2a", "#15b9ee", "#0f5997","#409188", "#911e20", "#1350ce", "#10e5b1", "#fff4d7", "#cb2582", "#ce00be",
"#32d5d6", "#17232", "#608572", "#c79bc2", "#00f87c", "#77772a", "#6995ba","#fc6b57", "#f07815", "#8fd883", "#060e27", "#96e591", "#21d52e", "#d00043",
"#911e7e", "#3f16d9", "#0f525f", "#ac7c0a", "#b4c086", "#c9d730", "#30cc49","#3d6751", "#fb4c03", "#640fc1", "#62c03e", "#d3493a", "#88aa0b", "#406df9",
"#615af0", "#4be47", "#2a3434", "#4a543f", "#79bca0", "#a8b8d4", "#00efd4","#7ad236", "#7260d8", "#1deaa7", "#06f43a", "#823c59", "#e3d94c", "#dc1c06",
"#f53b2a", "#b46238", "#2dfff6", "#a82b89", "#1a8011", "#436a9f", "#1a806a","#4cf09d", "#c188a2", "#67eb4b", "#b308d3", "#fc7e41", "#af3101", "#ff065",
"#71b1f4", "#a2f8a5", "#e23dd0", "#d3486d", "#00f7f9", "#474893", "#3cec35","#1c65cb", "#5d1d0c", "#2d7d2a", "#ff3420", "#5cdd87"

    };

        public FileProcessor()
        {
            NodeDictionary = JsonConvert.DeserializeObject<Dictionary<string, Node>>(File.ReadAllText(@"NodeDictionary.json"));
            WayHashMap = JsonConvert.DeserializeObject<Dictionary<string, Way>>(System.IO.File.ReadAllText(@"ways.json"));
            JunctionNodeHashMap = JsonConvert.DeserializeObject<Dictionary<string, JunctionNode>>(System.IO.File.ReadAllText(@"junctionNodes.json"));
        }

        //method which taking in the 
        public void getWaysFromNodes()
        {
            HashSet<JunctionNode> set = JsonConvert.DeserializeObject<HashSet<JunctionNode>>(File.ReadAllText(@"newpath.json"));
            HashSet<Way> ways = new HashSet<Way>();

            Dictionary<string, JunctionNode> pathDictionary = new Dictionary<string, JunctionNode>();
            foreach (JunctionNode node in set)
            {
                pathDictionary.Add(node.Id, node);
            }

            foreach (KeyValuePair<string, JunctionNode> pair in pathDictionary)
            {
                foreach (KeyValuePair<string, string> node in pair.Value.wayToNodeMap)
                {
                    if (pathDictionary.ContainsKey(node.Value) && (
                            WayHashMap.GetValueOrDefault(node.Key).roadClass.Equals("trunk") ||
                            WayHashMap.GetValueOrDefault(node.Key).roadClass.Equals("trunk_link")))
                    {
                        ways.Add(WayHashMap.GetValueOrDefault(node.Key));
                    }
                }
            }
            File.WriteAllText("demoSubTreeConnection.json", JsonConvert.SerializeObject(ways.ToList()));

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
                    if ((res >= 3) || (res == 2 && !node.Value.roadTypes.Contains("motorway_link")) || (res == 1 && node.Value.roadTypes.Count == 1))
                    {
                        DisconnectionNode disconnectionNode = new DisconnectionNode();
                        disconnectionNode.Id = node.Value.Id;
                        disconnectionNode.Lat = node.Value.Lat;
                        disconnectionNode.Lng = node.Value.Lng;
                        List<Way> w = new List<Way>();
                        foreach (KeyValuePair<string, string> road in node.Value.wayToNodeMap)
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
            Dictionary<int, HashSet<int>> checker = new Dictionary<int, HashSet<int>>();

            //dictionary having Key= label value and Value = set of subtree for that label
            Dictionary<int, HashSet<JunctionNode>> LabelToSubtrees = new Dictionary<int, HashSet<JunctionNode>>();

            //queue for the children nodes
            Queue children;

            int currentLabel = 0;

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

                    while (children.Count != 0)
                    {
                        JunctionNode currentNode = (JunctionNode)children.Dequeue();

                        //look thru all the ways this particular node is present in
                        foreach (string way in NodeDictionary.GetValueOrDefault(currentNode.Id).ways)
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
                                    if (label != 0)
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
                                if (label != currentLabel)
                                {
                                    //if the node is already labeled
                                    if (label != 0)
                                    {
                                        //append the entire subtree of that label to that of the currentlabel and update their label to currentlabel
                                        foreach (JunctionNode junctionNode in LabelToSubtrees.GetValueOrDefault(label))
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
            foreach (int label in LabelsToBeRemoved)
            {
                LabelToSubtrees.Remove(label);
            }

            connectSubGraphs(LabelToSubtrees, new List<string>() { "motorway" }, new List<string>() { "trunk", "trunk_link","primary","primary_link", "motorway_link", "fares" });

            //-------------------Code to color code the sub graphs starts below---------------------------//


            //Set of all disjoint graphs color coded 
            DisjointedSubTreeWays = new HashSet<Way>();

            //loop over all ways
            foreach (KeyValuePair<string, Way> way in WayHashMap)
            {
                //loop over all the disjointed hashset
                foreach (int labelNodes in LabelToSubtrees.Keys)
                {

                    //check which graph the way belongs to
                    if (LabelToSubtrees.GetValueOrDefault(labelNodes).Contains(JunctionNodeHashMap.GetValueOrDefault(way.Value.startNode.Id)))
                    {
                        if (roadClassification.Contains(way.Value.roadClass))
                        {
                            //color code it accordingly
                            way.Value.colorCode = ColorValues[labelNodes];
                            //add it to the set which will be converted to a json file
                            DisjointedSubTreeWays.Add(way.Value);
                        }
                    }

                }
            }



            //------------------------Code to color code the sub graphs ends here-----------------------//

            //write it to the file
            File.WriteAllText(fileName, JsonConvert.SerializeObject(DisjointedSubTreeWays.ToList()));


        }


        private void connectSubGraphs(Dictionary<int, HashSet<JunctionNode>> LabelToSubtrees, List<string> subgraphRoadClasses, List<string> allowedPathRoadClasses)
        {
            Dictionary<int, HashSet<JunctionNode>> LabelToOutgoingEndPoint = new Dictionary<int, HashSet<JunctionNode>>();
            Dictionary<int, HashSet<JunctionNode>> LabelToIncomingEndPoint = new Dictionary<int, HashSet<JunctionNode>>();

            //this hashet will contain the incoming endpoint nodes of all of the subgraphs
            HashSet<JunctionNode> incomingEnpointNodes = new HashSet<JunctionNode>();


            //traverse thru all the keys of the labelToSubtrees
            foreach (int label in LabelToSubtrees.Keys)
            {
                // add the outgoing nodes that make up a subgraph
                HashSet<JunctionNode> set = LabelToOutgoingEndPoint.GetValueOrDefault(label, new HashSet<JunctionNode>());

                //traverse thru its nodes
                foreach (JunctionNode nd in LabelToSubtrees.GetValueOrDefault(label))
                {
                    //Node node = NodeDictionary.GetValueOrDefault(nd.Id);
                    // checking what road types is this node connected to from the NodeDictionary and not LabelTosubtree because we handle the 'T intersection' problem there
                    foreach (string way in nd.wayToNodeMap.Keys)
                    {
                        // if we are at a road class that the subtree is out made of
                        if (subgraphRoadClasses.Contains(WayHashMap.GetValueOrDefault(way).roadClass))
                        {
                            // if oneWay = yes or null (we assume null means it is oneway)
                            if (!WayHashMap.GetValueOrDefault(way).oneWay.Equals("no"))
                            {

                                // if the node is an outgoing or incoming endpoint of the subgraph
                                if (nd.roadTypes.Intersect(subgraphRoadClasses).Any() && (nd.roadTypes.Intersect(allowedPathRoadClasses).Any()))//.Except(new List<string>() { "motorway_link" })).Any()))
                                {
                                    //if it is an incoming node into the subgraph
                                    if (WayHashMap.GetValueOrDefault(way).startNode.Id.Equals(nd.Id))
                                    {
                                        //add to incoming nodes
                                        incomingEnpointNodes.Add(nd);
                                    }
                                    // if it is an outgoing node from the subgraph
                                    else if (WayHashMap.GetValueOrDefault(way).endNode.Id.Equals(nd.Id))
                                    {
                                        //add it to outgoing nodes of that subgrapg
                                        set.Add(nd);
                                        LabelToOutgoingEndPoint[label] = set;
                                    }

                                }
                            }
                            // if oneway = no
                            else
                            {
                                incomingEnpointNodes.Add(nd);
                                set.Add(nd);
                                LabelToOutgoingEndPoint[label] = set;
                            }
                        }
                    }
                }
            }

            foreach (JunctionNode node in incomingEnpointNodes)
            {
                HashSet<JunctionNode> set = LabelToIncomingEndPoint.GetValueOrDefault(node.label, new HashSet<JunctionNode>());
                set.Add(node);
                LabelToIncomingEndPoint[node.label] = set;
            }



            //-------------------------------------- BELOW IS WHERE THE BIG BOI ALGORITHM STARTS -------------------------------------------

            //int sourcelabel = 5;
            //List<List<JunctionNode>> AtoBpaths = BFSHelper(subgraphRoadClasses, allowedPathRoadClasses, LabelToOutgoingEndPoint, incomingEnpointNodes, sourcelabel);
            //int targetLabel = AtoBpaths.FirstOrDefault().LastOrDefault().label;
            //List<List<JunctionNode>> BtoApaths = BFSHelperWithTargetSubtree(subgraphRoadClasses, allowedPathRoadClasses, LabelToOutgoingEndPoint, incomingEnpointNodes, sourcelabel, targetLabel);


            //HashSet<JunctionNode> sett = LabelToSubtrees.GetValueOrDefault(sourcelabel).ToHashSet();
            //sett = sett.Union(LabelToSubtrees.GetValueOrDefault(targetLabel)).ToHashSet();
            //sett.RemoveWhere(x => (x.roadTypes.Contains("motorway_link") && !x.roadTypes.Contains("motorway")));

            //sett = sett.Union(AtoBpaths.SelectMany(x=> x).ToHashSet()).ToHashSet();
            //sett = sett.Union(BtoApaths.SelectMany(x => x).ToHashSet()).ToHashSet();

            ////While debbuging; Its normal if the number of nodes is less than the one in labelToSubtrees because we are removing motorway links
            //List<List<string>> result = generateStronglyDisconnectedComponents(sett, allowedPathRoadClasses.Union(subgraphRoadClasses).ToList());


            
            // Create the main graph and the aggregate label list of that graph
            HashSet<JunctionNode> currentGraph = new HashSet<JunctionNode>();           
            List<int> currentGraphAggregateLabels =  new List<int>();

            int currentGraphLabel = 5;

            //Assign the first graph to the main graph and add its key to the aggregate list
            currentGraph = LabelToOutgoingEndPoint.GetValueOrDefault(currentGraphLabel);
            currentGraphAggregateLabels.Add(currentGraphLabel);

            //remove the current graph from labelToOutgoing
            LabelToOutgoingEndPoint.Remove(currentGraphLabel);

            while (LabelToOutgoingEndPoint.Count > 0)
            {
                
                //go from currentGraph to the closes graph it finds, and the other way around
                List<List<JunctionNode>> AtoBpaths = BFSHelper(subgraphRoadClasses, allowedPathRoadClasses, currentGraph, incomingEnpointNodes, currentGraphAggregateLabels);

                //if currentgraph is able to connect to a neiboring subgraph
                if (AtoBpaths.Count > 0)
                {
                    int targetLabel = AtoBpaths.FirstOrDefault().LastOrDefault().label;
                    Console.WriteLine("=================================================>" + targetLabel);
                    List<List<JunctionNode>> BtoApaths = BFSHelperWithTargetSubtree(subgraphRoadClasses, allowedPathRoadClasses, LabelToOutgoingEndPoint, incomingEnpointNodes, currentGraphAggregateLabels, targetLabel);

                    //Unioning AtoB path, BtoA path and target graph to the current graph
                    currentGraph = currentGraph.Union(AtoBpaths.SelectMany(x => x).ToHashSet()).ToHashSet();
                    currentGraph = currentGraph.Union(BtoApaths.SelectMany(x => x).ToHashSet()).ToHashSet();
                    HashSet<JunctionNode> targetGraph = LabelToOutgoingEndPoint.GetValueOrDefault(targetLabel);
                    currentGraph.Union(targetGraph);

                    //removing the target graph label and adding it to the currentGraphAggregateLabels
                    LabelToOutgoingEndPoint.Remove(targetLabel);
                    currentGraphAggregateLabels.Add(targetLabel);
                }
                //if we cannot reach any unvisisted subgraph (if we visited everything in current island)
                else
                {
                    //Assign the first graph to the main graph and add its key to the aggregate list
                    currentGraph = LabelToOutgoingEndPoint.FirstOrDefault().Value.ToHashSet();
                    currentGraphAggregateLabels.Add(LabelToOutgoingEndPoint.FirstOrDefault().Key);

                    //remove the current graph from labelToOutgoing 
                    currentGraphLabel = LabelToOutgoingEndPoint.FirstOrDefault().Key;
                    LabelToOutgoingEndPoint.Remove(currentGraphLabel);
                }
            }
            File.WriteAllText("currentgraph_primary.json", JsonConvert.SerializeObject(currentGraph));
            
            
            //HashSet<JunctionNode>
            currentGraph = JsonConvert.DeserializeObject<HashSet<JunctionNode>>(File.ReadAllText(@"currentgraph.json"));
            HashSet<JunctionNode> sett = LabelToSubtrees.Values.SelectMany(x => x).ToHashSet();
            sett.RemoveWhere(x => (x.roadTypes.Contains("motorway_link") && !x.roadTypes.Contains("motorway")));
            sett = sett.Union(currentGraph).ToHashSet();
            //File.WriteAllText("union.json", JsonConvert.SerializeObject(sett));
            List<List<string>> result = generateStronglyDisconnectedComponents(sett, allowedPathRoadClasses.Union(subgraphRoadClasses).ToList());
            File.WriteAllText("result.json", JsonConvert.SerializeObject(result));

        }

        public List<string> motorwayMotorwayLink = new List<string>{ "motorway", "motorway_link" };

        //Here is where we run the BFS on the endpoint nodes of the graphs, return sorted list of shortest paths to closest subtrees 
        private List<List<JunctionNode>> BFSHelper(List<string> subgraphRoadClasses, List<string> allowedPathRoadClasses,HashSet<JunctionNode> sourceGraph, HashSet<JunctionNode> incomingEnpointNodes,List<int> subtreeLabels)
        {
            List<List<JunctionNode>> pathsOfSubtree = new List<List<JunctionNode>>();

            //traverse thru its hashset
            foreach (JunctionNode node in sourceGraph)
            {

                //Console.WriteLine("node: "+ node.Id);

                //queue for the children nodes
                Queue children = new Queue();
                children.Enqueue(new List<JunctionNode>() { node });

                HashSet<string> visitedNodes = new HashSet<string>();

                while (children.Count != 0)
                {
                    List<JunctionNode> path = (List<JunctionNode>)children.Dequeue();
                    JunctionNode currentNode = path.Last();

                    //look thru all the ways this particular node is present in
                    foreach (string way in NodeDictionary.GetValueOrDefault(currentNode.Id).ways)
                    {
                     

                        if(!subgraphRoadClasses.Contains(WayHashMap.GetValueOrDefault(way).roadClass))
                        {
                            //if we want to achieve connectivity only using trunk and trunklinks
                            if (allowedPathRoadClasses.Contains(WayHashMap.GetValueOrDefault(way).roadClass))
                            {
                                JunctionNode startNode = JunctionNodeHashMap.GetValueOrDefault(WayHashMap.GetValueOrDefault(way).startNode.Id);
                                JunctionNode endNode = JunctionNodeHashMap.GetValueOrDefault(WayHashMap.GetValueOrDefault(way).endNode.Id);                             

                                //if we are at a roundabout
                                if(startNode.Id.Equals(endNode.Id) && !visitedNodes.Contains(startNode.Id))
                                {
                                    foreach(Node nd in WayHashMap.GetValueOrDefault(way).nodes)
                                    {
                                        //if one of the middle nodes of the intersection is connected to another way
                                        if(nd.ways.Count > 1)
                                        {
                                            JunctionNode junctionNode = JunctionNodeHashMap.GetValueOrDefault(nd.Id);

                                            //if we reach the first node of another subtree
                                            if (!subtreeLabels.Contains(junctionNode.label) && junctionNode.roadTypes.Intersect(subgraphRoadClasses).Any() && incomingEnpointNodes.Contains(junctionNode))
                                            {
                                                Console.WriteLine(path.Count);
                                                path.Add(startNode);

                                                path.Add(junctionNode);
                                                //File.WriteAllText("newpath.json", JsonConvert.SerializeObject(path.ToList()));
                                                pathsOfSubtree.Add(path);
                                                goto end_of_while_loop;
                                            }

                                            List<JunctionNode> new_path = new List<JunctionNode>();
                                            new_path.AddRange(path);
                                            new_path.Add(startNode);
                                            new_path.Add(junctionNode);
                                            children.Enqueue(new_path);
                                            
                                        }
                                    }
                                    
                                }

                                //if we aren't at a roundabout
                                else
                                {

                                    // if we the way.oneway = yes or is null (we assume if oneWay = null that it means oneWay = yes )
                                    if((!WayHashMap.GetValueOrDefault(way).oneWay.Equals("no") && motorwayMotorwayLink.Contains(WayHashMap.GetValueOrDefault(way).roadClass)) || WayHashMap.GetValueOrDefault(way).oneWay.Equals("yes"))
                                    {
                                        // if it is one way, then we do not traverse from endnode to startnode because it would violate the oneWay direction
                                        if(currentNode.Id.Equals(WayHashMap.GetValueOrDefault(way).endNode.Id))
                                        {
                                            continue;
                                        }

                                        // if the direction is respected (from startnode to endnode)
                                        if (!endNode.Id.Equals(currentNode.Id) && !visitedNodes.Contains(endNode.Id))
                                        {
                                            //if we reach the first node of another subtree
                                            if (!subtreeLabels.Contains(endNode.label) && endNode.roadTypes.Intersect(subgraphRoadClasses).Any() && incomingEnpointNodes.Contains(endNode))
                                            {
                                                path.Add(endNode);
                                                pathsOfSubtree.Add(path);
                                                goto end_of_while_loop;
                                            }
                                            List<JunctionNode> new_path = new List<JunctionNode>();
                                            new_path.AddRange(path);
                                            new_path.Add(endNode);
                                            children.Enqueue(new_path);
                                        }
                                    }
                                    // if oneway = no
                                    else
                                    {
                                        if (!startNode.Id.Equals(currentNode.Id) && !visitedNodes.Contains(startNode.Id))
                                        {
                                            //if we reach the first node of another subtree
                                            if (!subtreeLabels.Contains(startNode.label) && startNode.roadTypes.Intersect(subgraphRoadClasses).Any() && incomingEnpointNodes.Contains(startNode))
                                            {
                                                path.Add(startNode);
                                                pathsOfSubtree.Add(path);
                                                goto end_of_while_loop;
                                            }
                                            List<JunctionNode> new_path = new List<JunctionNode>();
                                            new_path.AddRange(path);
                                            new_path.Add(startNode);
                                            children.Enqueue(new_path);
                                        }

                                        if (!endNode.Id.Equals(currentNode.Id) && !visitedNodes.Contains(endNode.Id))
                                        {
                                            //if we reach the first node of another subtree
                                            if (!subtreeLabels.Contains(endNode.label) && endNode.roadTypes.Intersect(subgraphRoadClasses).Any() && incomingEnpointNodes.Contains(endNode))
                                            {
                                                path.Add(endNode);
                                                pathsOfSubtree.Add(path);
                                                goto end_of_while_loop;
                                            }
                                            List<JunctionNode> new_path = new List<JunctionNode>();
                                            new_path.AddRange(path);
                                            new_path.Add(endNode);
                                            children.Enqueue(new_path);
                                        }
                                    }        
                                }
                            }
                        }
                    }
                    visitedNodes.Add(currentNode.Id);
                }
                end_of_while_loop: { }
            }
            return pathsOfSubtree.OrderBy(a => a.Count).ToList();
        }

        //Same as BFSHelper but we specify what subtree to hit, returns sorted list of shortest paths to target subtree
        private List<List<JunctionNode>> BFSHelperWithTargetSubtree(List<string> subgraphRoadClasses, List<string> allowedPathRoadClasses, Dictionary<int, HashSet<JunctionNode>> LabelToOutgoingEndPoint, HashSet<JunctionNode> incomingEnpointNodes, List<int> targetGraphLabels, int  subtreeLabel)
        {
            List<List<JunctionNode>> pathsOfSubtree = new List<List<JunctionNode>>();

            //traverse thru its hashset
            foreach (JunctionNode node in LabelToOutgoingEndPoint.GetValueOrDefault(subtreeLabel))
            {

                //Console.WriteLine("node: " + node.Id);

                //queue for the children nodes
                Queue children = new Queue();
                children.Enqueue(new List<JunctionNode>() { node });

                HashSet<string> visitedNodes = new HashSet<string>();

                while (children.Count != 0)
                {
                    List<JunctionNode> path = (List<JunctionNode>)children.Dequeue();
                    JunctionNode currentNode = path.Last();

                    //look thru all the ways this particular node is present in
                    foreach (string way in NodeDictionary.GetValueOrDefault(currentNode.Id).ways)
                    {
                        if (!subgraphRoadClasses.Contains(WayHashMap.GetValueOrDefault(way).roadClass))
                        {
                            //if we want to achieve connectivity only using trunk and trunklinks
                            if (allowedPathRoadClasses.Contains(WayHashMap.GetValueOrDefault(way).roadClass))
                            {
                                JunctionNode startNode = JunctionNodeHashMap.GetValueOrDefault(WayHashMap.GetValueOrDefault(way).startNode.Id);
                                JunctionNode endNode = JunctionNodeHashMap.GetValueOrDefault(WayHashMap.GetValueOrDefault(way).endNode.Id);

                                //if we are at a roundabout
                                if (startNode.Id.Equals(endNode.Id) && !visitedNodes.Contains(startNode.Id))
                                {
                                    foreach (Node nd in WayHashMap.GetValueOrDefault(way).nodes)
                                    {
                                        //if one of the middle nodes of the intersection is connected to another way
                                        if (nd.ways.Count > 1)
                                        {
                                            JunctionNode junctionNode = JunctionNodeHashMap.GetValueOrDefault(nd.Id);

                                            //if we reach the first node of another subtree
                                            if (targetGraphLabels.Contains(junctionNode.label) && junctionNode.label != subtreeLabel && junctionNode.roadTypes.Intersect(subgraphRoadClasses).Any() && incomingEnpointNodes.Contains(junctionNode))
                                            {
                                                path.Add(startNode);
                                                path.Add(junctionNode);
                                               
                                                pathsOfSubtree.Add(path);
                                                goto end_of_while_loop;
                                            }
                                            List<JunctionNode> new_path = new List<JunctionNode>();
                                            new_path.AddRange(path);
                                            new_path.Add(startNode);
                                            new_path.Add(junctionNode);
                                            children.Enqueue(new_path);

                                        }
                                    }

                                }

                                //if we aren't at a roundabout
                                else
                                {
                                    // if we the way.oneway = yes or is null (we assume if oneWay = null that it means oneWay = yes )
                                    if ((!WayHashMap.GetValueOrDefault(way).oneWay.Equals("no") && motorwayMotorwayLink.Contains(WayHashMap.GetValueOrDefault(way).roadClass)) || WayHashMap.GetValueOrDefault(way).oneWay.Equals("yes"))
                                    {
                                        // if it is one way, then we do not traverse from endnode to startnode because it would violate the oneWay direction
                                        if (currentNode.Id.Equals(WayHashMap.GetValueOrDefault(way).endNode.Id))
                                        {
                                            continue;
                                        }

                                        // if the direction is respected (from startnode to endnode)
                                        if (!endNode.Id.Equals(currentNode.Id) && !visitedNodes.Contains(endNode.Id))
                                        {
                                            //if we reach the first node of another subtree
                                            if (targetGraphLabels.Contains(endNode.label) && endNode.label != subtreeLabel && endNode.roadTypes.Intersect(subgraphRoadClasses).Any() && incomingEnpointNodes.Contains(endNode))
                                            {
                                                path.Add(endNode);
                                                pathsOfSubtree.Add(path);
                                                goto end_of_while_loop;
                                            }
                                            List<JunctionNode> new_path = new List<JunctionNode>();
                                            new_path.AddRange(path);
                                            new_path.Add(endNode);
                                            children.Enqueue(new_path);
                                        }
                                    }
                                    // if oneway = no
                                    else
                                    {
                                        if (!startNode.Id.Equals(currentNode.Id) && !visitedNodes.Contains(startNode.Id))
                                        {
                                            //if we reach the first node of another subtree
                                            if (targetGraphLabels.Contains(startNode.label) && startNode.label != subtreeLabel && startNode.roadTypes.Intersect(subgraphRoadClasses).Any() && incomingEnpointNodes.Contains(startNode))
                                            {
                                                path.Add(startNode);
                                                pathsOfSubtree.Add(path);
                                                goto end_of_while_loop;
                                            }
                                            List<JunctionNode> new_path = new List<JunctionNode>();
                                            new_path.AddRange(path);
                                            new_path.Add(startNode);
                                            children.Enqueue(new_path);
                                        }

                                        if (!endNode.Id.Equals(currentNode.Id) && !visitedNodes.Contains(endNode.Id))
                                        {
                                            //if we reach the first node of another subtree
                                            if (targetGraphLabels.Contains(endNode.label) && endNode.label != subtreeLabel && endNode.roadTypes.Intersect(subgraphRoadClasses).Any() && incomingEnpointNodes.Contains(endNode))
                                            {
                                                Console.WriteLine(path.Count);
                                                path.Add(endNode);
                                                //File.WriteAllText("newpath.json", JsonConvert.SerializeObject(path.ToList()));
                                                pathsOfSubtree.Add(path);
                                                goto end_of_while_loop;
                                            }
                                            List<JunctionNode> new_path = new List<JunctionNode>();
                                            new_path.AddRange(path);
                                            new_path.Add(endNode);
                                            children.Enqueue(new_path);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    visitedNodes.Add(currentNode.Id);
                }
            end_of_while_loop: { }
            }
            return pathsOfSubtree.OrderBy(a => a.Count).ToList();
        }

        // kosaraju modified algorithm to check strongly connected components
        private List<List<string>> generateStronglyDisconnectedComponents(HashSet<JunctionNode> graph, List<string> graphRoadTypes)
        {
            GraphTarjan g = new GraphTarjan();

            Dictionary<string, NodeTarjan> d = new Dictionary<string, NodeTarjan>();
            Dictionary<string, int> testcount = new Dictionary<string, int>();
            
            //populating g.V, and g.Adj
            foreach(JunctionNode node in graph)
            {
               
                if (!d.ContainsKey(node.Id))
                {
                    NodeTarjan nd = new NodeTarjan(node.Id);
                    g.V.Add(nd);
                    d.Add(node.Id, nd);
                    g.Adj[nd] = new HashSet<NodeTarjan>();
                }
               
            }

          
            foreach (JunctionNode node in graph)
            {         
                HashSet<NodeTarjan> set = g.Adj.GetValueOrDefault(d.GetValueOrDefault(node.Id));

                foreach (string wayTemp in NodeDictionary.GetValueOrDefault(node.Id).ways)
                {
                    Way way = WayHashMap.GetValueOrDefault(wayTemp);

                   // we make sure that the way is part of the graph
                   if(graphRoadTypes.Contains(way.roadClass) && graph.Contains(JunctionNodeHashMap.GetValueOrDefault(way.startNode.Id)) && graph.Contains(JunctionNodeHashMap.GetValueOrDefault(way.endNode.Id)))
                   {
                        //if its a roundabout
                        if(way.startNode.Id.Equals(way.endNode.Id))
                        {
                            //add the roundabout startnode to the adjacency list of current node
                            set.Add(d.GetValueOrDefault(way.startNode.Id));

                            // Now i have to add current node to the adjacency list of the roundabout startnode (reverse edge)
                            // but before doing that i check if it already exists as an entry in graph's V NodeTarjan
                            HashSet<NodeTarjan> roundaboutSet = g.Adj.GetValueOrDefault(d.GetValueOrDefault(way.startNode.Id));
                            roundaboutSet.Add(d.GetValueOrDefault(node.Id));
                            g.Adj[d.GetValueOrDefault(way.startNode.Id)] = roundaboutSet;
                           
                        }
                        else
                        {
                            // if we the way.oneway = yes or is null (we assume if oneWay = null that it means oneWay = yes )
                            if ((!way.oneWay.Equals("no") && motorwayMotorwayLink.Contains(way.roadClass)) || way.oneWay.Equals("yes")) 
                            {
                                if (node.Id.Equals(way.startNode.Id))
                                {
                                    set.Add(d.GetValueOrDefault(way.endNode.Id));
                                }  
                            }
                            //if oneWay = no
                            else
                            {
                                //if my current node is 
                                if(way.startNode.Id.Equals(node.Id))
                                {
                                    set.Add(d.GetValueOrDefault(way.endNode.Id));
                                }
                                else
                                {
                                    set.Add(d.GetValueOrDefault(way.startNode.Id));
                                }
                            }
                        }
                   }
                }

                //add the adjacency list
                g.Adj[d.GetValueOrDefault(node.Id)] = set;
            }


            //foreach (KeyValuePair<NodeTarjan,HashSet<NodeTarjan>> tarzan in g.Adj)
            //{
            //    Console.WriteLine(tarzan.Key.N + " " + tarzan.Value.Count);
            //    if(tarzan.Key.N.Equals("3318019879") || tarzan.Key.N.Equals("2097666539"))
            //    {

            //    }
            //}

            //run the algorithm
            return g.Tarjan();
            
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
