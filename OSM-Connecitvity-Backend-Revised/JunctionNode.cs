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

        //Added in SUM
        //relationid,role signifying which (from,to,via) are we at while exploring the node
        //This will work for only no retsrictions 
        //{key:rlnid, Value = [counter,role]}
        //COUNTER is not required as we do not use it, need to modify to Dictionary<string,string> - {key:rlnid, Value = role}
        //public Dictionary<string, List<string>> restrictions { get; set; }
        public Dictionary<string, string> restrictions { get; set; }

        public JunctionNode(string id, Dictionary<string,string> wayToNodeMap, List<string> roadTypes, float Lat, float Lng)
        {
            this.Id = id;
            this.wayToNodeMap = wayToNodeMap;
            this.roadTypes = roadTypes;
            this.Lat = Lat;
            this.Lng = Lng;
            this.restrictions = new Dictionary<string, string>();
        }

        public JunctionNode()
        {
            this.roadTypes = new List<string>();
            this.wayToNodeMap = new Dictionary<string, string>();
            this.restrictions = new Dictionary<string, string>();
        }
    }
}
