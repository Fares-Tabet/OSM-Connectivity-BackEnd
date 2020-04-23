using System;
using System.Collections.Generic;
using System.Text;

namespace OSM_Connecitvity_Backend_Revised
{
    class Way
    {
        public string Id { get; set; }
        public string roadClass { get; set; }
        public string maxSpeed { get; set; }
        public string oneWay { get; set; }
        public string name { get; set; }
        public Node startNode  { get; set; }
        public Node endNode { get; set; }
        public List<Node> nodes { get; set; }
        public string colorCode { get; set; }
        public Way()
        {
            
        }
        public Way(string Id, string roadClass, Node endNode, Node startNode, List<Node> nodes)
        {
            this.Id = Id;

            this.roadClass = roadClass;

            this.nodes = nodes;

            this.endNode = endNode;

            this.startNode = startNode;
        }

    }
}
