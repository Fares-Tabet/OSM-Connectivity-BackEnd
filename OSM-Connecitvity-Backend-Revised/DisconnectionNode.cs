using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OSM_Connecitvity_Backend_Revised
{
    class DisconnectionNode
    {
        public string Id { get; set; }
        public float Lat { get; set; }
        public float Lng { get; set; }

        public List<Way> roads { get; set; }

        public DisconnectionNode()
        {
        }
    }
}
