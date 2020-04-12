using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OSM_Connecitvity_Backend_Revised
{
    class Node
    {
        public string Id { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }

        public Node(string id, float lat, float lon)
        {
            this.Id = id;
            this.Lat = lat;
            this.Lng = lon;
        }


    }
}
