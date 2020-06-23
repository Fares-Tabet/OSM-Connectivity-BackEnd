using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;


namespace OSM_Connecitvity_Backend_Revised
{
    public class RelationFileProcessor
    {

        public string OsmFilePath;
        

        public RelationFileProcessor(string OsmFilePath)
        {
            this.OsmFilePath = OsmFilePath;
        }

        Dictionary<string, Relation> relationDictionary = new Dictionary<string, Relation>();

        //K:considering restrictions only and not restrictions for vehicles, as we r interested in turn restrictions

        public void extractRelationsFromOSM()
        {
            XDocument doc = XDocument.Load(OsmFilePath);
            List<XElement> elements = doc.Descendants("relation").ToList();
            foreach (XElement el in elements)
            {

                string relationId= el.FirstAttribute.Value;
                List<string> restrictions = new List<string>();
                List<KeyValuePair<string, string>> memberWays = new List<KeyValuePair<string, string>>();
                Dictionary<string, string> memberNodes = new Dictionary<string, string>();

                //Will it have more than one restriction. Possible yes. if we consider other types too
                //but here we consider no and only, can they be together

                //Should we add the other tags like members of a relation if there is no restriction
                // Here I am adding even when no restrictions exist,
                //coz we have filtered xml such that relations with restrictions are only kept

                // Should we consider only when from to or from to via are present,
                //i.e if from or to is missinng shall we leave out
                //(Done)
                
                List<XElement> restrictionTags = el.Descendants("tag").Where(x => (string)x.Attribute("k") == "restriction").ToList();
                if(restrictionTags.Count != 0)
                {
                    foreach (XElement restrElement in restrictionTags)
                    {
                        restrictions.Add(restrElement.Attribute("v").Value.ToString());
                    }
                    
                }


                // Should we add the restrictions for all road types or filter with only required ones:
                //Yes 

                // Also there are some relations with semantical meaning eg: wayid 83262120 in 2 relations with same
                //restriction but diff nodes and ways and roles.
                // if no from shall we leave out that relation
                foreach (XElement memElement in el.Descendants("member").ToList())
                {
                    string memberType = memElement.Attribute("type").Value.ToString();
                    if (memberType.Equals("way"))
                    {
                        //TODO: check if this is present in the way already and if it is same,
                        // Ask if all relations will be populated here on in the createDataFiles in ways
                        // Suggest: Add way relations while processing ways
                        // When adding here cross check if it was already added
                        // what format should be the relation id be for those in ways and not in relations
                        // if we can format related to ref it wud be easy to check eg 'rel123456a' where 123456 is wayid
                        //Console.WriteLine("member tag" + memElement.Attribute("ref").Value.ToString());
                        //Exception: For U turns we have one way twice in relation like from and to

                        //TODO: check if this way or node is present
                        //string thisWayId = memElement.Attribute("ref").Value.ToString();
                        //if(wayDictionary.GetValueOrDefault(thisWayId)){
                        //  memberWays.Add(new KeyValuePair<string, string>(thisWayId, memElement.Attribute("role").Value.ToString()));

                        //}
                        memberWays.Add(new KeyValuePair<string, string>(memElement.Attribute("ref").Value.ToString(), memElement.Attribute("role").Value.ToString()));
                    }
                    else if (memberType.Equals("node"))
                    {
                        //TODO: check if this way or node is present
                        //string thisNodeId = memElement.Attribute("ref").Value.ToString();
                        //if(NodeDictionary.GetValueOrDefault(thisNodeId)){
                        //   memberNodes.Add(thisNodeId, memElement.Attribute("role").Value.ToString());
                        //}


                        memberNodes.Add(memElement.Attribute("ref").Value.ToString(), memElement.Attribute("role").Value.ToString());

                    }

                }

                relationDictionary.Add(relationId,
                        new Relation(relationId, restrictions, memberWays, memberNodes));

            }

            File.WriteAllText("fiji_changed_Relations.json", JsonConvert.SerializeObject(relationDictionary));
            Console.WriteLine("Successfully written relations in json file");
        }
    }
}
