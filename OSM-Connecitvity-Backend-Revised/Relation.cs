/******************************************************************************
 *  Name:    Sikha Pentyala
 *  E-Mail:   sikha@uw.edu
 *  Proj: SUM: OSMConnectivity
 *
 *  Description: A user-defined data structure for relation nodes in OSM file.
 *
 ******************************************************************************/


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OSM_Connecitvity_Backend_Revised
{
    public class Relation
    {
        public string Id; //uniquely identifies relation node in XML

        //List of all member ways in this relation and its role (from , to, via)
        // {(wayid1,from),(wayid1,to) (wayid2,to)}

        //public Dictionary<string,string> memberWaysWithRole;
        // we can use list to have duplicates but what about lookup time?
        public List<KeyValuePair<string, string>> memberWaysWithRole;

        //List of all member nodes in this relation and its role (from , to, via)
        // {(nodeid1,from), (nodeid2,to)} 
        public Dictionary<string, string> memberNodesWithRole;

        //List of all restrictions
        // Look for tag k='restriction', v=''. This will store all v-s
        // We are interested in only specific type of restrictions?
        public List<string> typeOfRestrictions;

        public Relation()
        {
     
            this.typeOfRestrictions = new List<string>();
            this.memberWaysWithRole = new List<KeyValuePair<string, string>>();
            this.memberNodesWithRole = new Dictionary<string, string>();
        }

        public Relation(string Id, List<string> typeOfRestrictions,
            List<KeyValuePair<string, string>> wayRoles, Dictionary<string, string> nodeRoles)
        {
            this.Id = Id;
            this.typeOfRestrictions = typeOfRestrictions;
            this.memberWaysWithRole = wayRoles;
            this.memberNodesWithRole = nodeRoles;
        }
    }
}
