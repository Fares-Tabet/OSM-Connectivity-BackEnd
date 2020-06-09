using System;
using System.Collections.Generic;

namespace OSM_Connecitvity_Backend_Revised
{
    class Program
    {
        static void Main(string[] args)
        {
            //string connectivityFile = "thai_results_fix.json";

            //FileParser parser = new FileParser("/Users/fares/Downloads/thailand_allroads.xml");
            FileParser parser = new FileParser("NZ_allroads.xml");
            parser.createDataFiles();


            FileProcessor fileProcessor = new FileProcessor();
            //fileProcessor.generateRoadNetwork(new List<string>() { "motorway", "motorway_link" }, "NZ_motoroway_RNG.json");
            //fileProcessor.generateRoadNetwork(new List<string>() { "trunk", "trunk_link" }, "NZ_trunk_RNG.json");

            //fileProcessor.generateIncorrectMotorwayConnections("NZ_IncorrectConnections.json");
            //fileProcessor.generateDisconnectionsDataBFS(new List<string>() { "motorway"}, "NZ_disconnections.json");
            //fileProcessor.suggestConnectivityFixBasedOnLeastAmountOfNodes(new List<string>() { "motorway", "motorway_link" }, new List<string>() { "trunk", "trunk_link", "motorway_link","primary","primary_link", "fares" }, connectivityFile);
            //fileProcessor.getWaysFromNodes(connectivityFile, "NZ_T-TL-Fix.json");

        }
    }
}
