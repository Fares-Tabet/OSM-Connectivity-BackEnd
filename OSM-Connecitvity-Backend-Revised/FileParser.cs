using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OSM_Connecitvity_Backend_Revised
{
    class FileParser
    {
        public string OsmFilePath;

        public FileParser(string OsmFilePath)
        {
            this.OsmFilePath = OsmFilePath;
        }

        public void createDataFiles()
        {
            XDocument doc = XDocument.Load(OsmFilePath);
            List<XElement> elements = doc.Descendants("way").ToList();

            //This dictionary contains the way objects and will be serialized into json and added to the DataFile folder
            Dictionary<string,Way> wayDictionary = new Dictionary<string, Way>();

            //This dictionary contains the junctionNOde objects and will be serialized into json and added to the DataFile folder
            Dictionary<string,JunctionNode> junctionNodeDictionary = new Dictionary<string, JunctionNode>();

            //create a list of way elements
            List<XElement> parentElements = new List<XElement>();
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
                    string[] coordinates = (getNodeLatLong(nd.FirstAttribute.Value)).Split(",");
                    Node node = new Node(nd.FirstAttribute.Value, float.Parse(coordinates[0]), float.Parse(coordinates[1]));
                    nodeList.Add(node);
                }

                way.nodes = nodeList;

                // popoulate the start and end node field of the way object
                XElement endPoint1 = el.Descendants("nd").ToList().First();
                string[] endPointCoordinates1 = (getNodeLatLong(endPoint1.FirstAttribute.Value)).Split(",");
                Node startNode = new Node(endPoint1.FirstAttribute.Value, float.Parse(endPointCoordinates1[0]), float.Parse(endPointCoordinates1[1]));
                way.startNode = startNode;

                XElement endPoint2 = el.Descendants("nd").ToList().Last();
                string[] endPointCoordinates2 = (getNodeLatLong(endPoint2.FirstAttribute.Value)).Split(",");
                Node endNode = new Node(endPoint2.FirstAttribute.Value, float.Parse(endPointCoordinates2[0]), float.Parse(endPointCoordinates2[1]));
                way.endNode = endNode;

                // populate the junctionNodeDictionary with the first and last node of each way
                JunctionNode firstJunction;
                if(junctionNodeDictionary.ContainsKey(startNode.Id))
                {
                    firstJunction = junctionNodeDictionary[startNode.Id];
                    firstJunction.roadTypes.Add(way.roadClass);
                    firstJunction.wayToNodeMap.Add(way.Id, endNode.Id);
                    junctionNodeDictionary[startNode.Id] = firstJunction;
                }
                else
                {
                    firstJunction = new JunctionNode(startNode.Id, new Dictionary<string, string>() { { way.Id, endNode.Id } }, new List<string>() { { way.roadClass } }, float.Parse(endPointCoordinates1[0]), float.Parse(endPointCoordinates1[1]));
                    junctionNodeDictionary.Add(startNode.Id, firstJunction);
                }

                JunctionNode lastJunction;
                if (junctionNodeDictionary.ContainsKey(endNode.Id))
                {
                    lastJunction = junctionNodeDictionary[endNode.Id];
                    lastJunction.roadTypes.Add(way.roadClass);
                    lastJunction.wayToNodeMap.Add(way.Id, endNode.Id);
                    junctionNodeDictionary[endNode.Id] = lastJunction;
                }
                else
                {
                    lastJunction = new JunctionNode(endNode.Id, new Dictionary<string, string>() { { way.Id, startNode.Id } }, new List<string>() { { way.roadClass } }, float.Parse(endPointCoordinates2[0]), float.Parse(endPointCoordinates2[1]));
                    junctionNodeDictionary.Add(endNode.Id, lastJunction);
                }

                wayDictionary.Add(way.Id,way);

            }

            // Create the way data file
            File.WriteAllText(@"~\..\..\..\..\DataFiles\ways.json", JsonConvert.SerializeObject(wayDictionary));

            // Create the junctionNodes data file
            File.WriteAllText(@"~\..\..\..\..\DataFiles\junctionNodes.json", JsonConvert.SerializeObject(junctionNodeDictionary));


        }

        // This method returns the latitude and longitude of a given node
        public string getNodeLatLong(string nodeID)
        {
            XDocument doc = XDocument.Load(OsmFilePath);
            XElement result = doc.Descendants("node")
                .FirstOrDefault(el => el.Attribute("id")?.Value == nodeID);

            return " " + result.Attribute("lat").Value + "," + result.Attribute("lon").Value;
        }
    }
}
