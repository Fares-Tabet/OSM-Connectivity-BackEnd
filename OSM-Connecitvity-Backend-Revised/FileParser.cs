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

        public FileParser(string OsmFilePath)
        {
            this.OsmFilePath = OsmFilePath;
        }

        public void createDataFiles()
        {
            populateNodeDictionary();
            XDocument doc = XDocument.Load(OsmFilePath);
            List<XElement> elements = doc.Descendants("way").ToList();
                
            //This dictionary contains the way objects and will be serialized into json and added to the DataFile folder
            Dictionary<string,Way> wayDictionary = new Dictionary<string, Way>();

            //This dictionary contains the junctionNOde objects and will be serialized into json and added to the DataFile folder
            Dictionary<string,JunctionNode> junctionNodeDictionary = new Dictionary<string, JunctionNode>();

            foreach (XElement el in elements)
            {
                Way way = new Way();

                // Populating way object 
                way.Id = el.FirstAttribute.Value;

                List<XElement> inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "highway").ToList();
                way.roadClass = inspectedTag.Count == 0 ? null : ((XElement)inspectedTag.First()).LastAttribute.Value;

                inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "name").ToList();
                way.name = inspectedTag.Count == 0 ? null : ((XElement)inspectedTag.First()).LastAttribute.Value;

                inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "maxspeed").ToList();
                way.maxSpeed = inspectedTag.Count == 0 ? null : ((XElement)inspectedTag.First()).LastAttribute.Value;

                inspectedTag = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "oneway").ToList();
                way.oneWay = inspectedTag.Count == 0 ? null : ((XElement)inspectedTag.First()).LastAttribute.Value;

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
                Console.WriteLine(way.Id);

            }

            // Create the way data file
            File.WriteAllText("ways.json", JsonConvert.SerializeObject(wayDictionary));

            // Create the junctionNodes data file
            File.WriteAllText("junctionNodes.json", JsonConvert.SerializeObject(junctionNodeDictionary));

            // Create the node data file
            File.WriteAllText("NodeDictionary.json", JsonConvert.SerializeObject(NodeDictionary));

        }

        public void populateNodeDictionary()
        {
            XDocument doc = XDocument.Load(OsmFilePath);
            List<XElement> elements = doc.Descendants("node").ToList();
            foreach (XElement el in elements)
            {
                String nodeid = el.Attribute("id").Value;
                Console.WriteLine(nodeid);
                Node node = new Node(nodeid, float.Parse(el.Attribute("lat").Value), float.Parse(el.Attribute("lon").Value));
                NodeDictionary.Add(nodeid, node);
            }
        }
    }
}
