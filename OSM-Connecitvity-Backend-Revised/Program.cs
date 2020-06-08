using System;
using System.Collections.Generic;

namespace OSM_Connecitvity_Backend_Revised
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectivityFile = "connectivityResutls.json";
            //, "primary", "primary_link",

            //FileParser parser = new FileParser("NZ_allRoads_latest.xml");
            //parser.createDataFiles();


            FileProcessor fileProcessor = new FileProcessor();

            //fileProcessor.generateIncorrectMotorwayConnections("NZ_IncorrectConnections.json");
            //fileProcessor.generateDisconnectionsDataBFS( new List<string>() { "motorway","motorway_link"}, "NZ_disconnections.json");           
            fileProcessor.suggestConnectivityFixBasedOnLeastAmountOfNodes(new List<string>() { "motorway", "motorway_link" }, new List<string>() { "trunk", "trunk_link", "motorway_link", "fares" }, connectivityFile);
            fileProcessor.getWaysFromNodes(connectivityFile, "NZ_T-TL-Fix.json");

        }
    }
}
