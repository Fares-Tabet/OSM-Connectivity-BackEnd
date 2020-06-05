using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OSM_Connecitvity_Backend_Revised
{
    class JunctionNode
    {
        public string Id { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }

        public Dictionary<string,string> wayToNodeMap { get; set; }

        public List<string> roadTypes { get; set; }
        public int label = 0;
        public JunctionNode(string id, Dictionary<string,string> wayToNodeMap, List<string> roadTypes, float Lat, float Lng)
        {
            this.Id = id;
            this.wayToNodeMap = wayToNodeMap;
            this.roadTypes = roadTypes;
            this.Lat = Lat;
            this.Lng = Lng;
    }

        public JunctionNode()
        {
            this.roadTypes = new List<string>();
            this.wayToNodeMap = new Dictionary<string, string>();
        }
    }
}
