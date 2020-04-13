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
        Dictionary<string, string> NodeDictionary;
        Dictionary<string, Way> WayHashMap;
        Dictionary<string, JunctionNode> JunctionNodeHashMap;

        public FileProcessor()
        {
            NodeDictionary = JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(@"NodeDictionary.json"));
            WayHashMap = JsonConvert.DeserializeObject<Dictionary<string, Way>>(System.IO.File.ReadAllText(@"ways.json"));
            JunctionNodeHashMap = JsonConvert.DeserializeObject<Dictionary<string, JunctionNode>>(System.IO.File.ReadAllText(@"junctionNodes.json"));
        }

        public void generateDisconnectionsData(string fileName)
		{
            List<DisconnectionNode> disconnectionNodes = new List<DisconnectionNode>();
            foreach(KeyValuePair<string, JunctionNode> node in JunctionNodeHashMap)
			{
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

        public void generateRoadNetwork(List<string> roadTypes,string fileName)
		{
            List<Way> ways = new List<Way>();
            foreach(KeyValuePair<string,Way> way in WayHashMap)
			{
				if (roadTypes.Contains(way.Value.roadClass))
				{
                    ways.Add(way.Value);
				}
			}
            File.WriteAllText(fileName, JsonConvert.SerializeObject(ways));
        }
    }
}
