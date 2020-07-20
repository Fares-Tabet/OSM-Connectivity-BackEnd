using System;
using System.Collections.Generic;

namespace OSM_Connecitvity_Backend_Revised
{
    class Program
    {
        static void Main(string[] args)
        {
            //string connectivityFile = "temp.json";

            //FileParser parser = new FileParser("/Users/sikha/OSM/nz_0714.xml");
            FileParser parser = new FileParser("/Users/sikha/OSM/fj_0714.xml");
            //parser.createDataFiles();


            FileProcessor fileProcessor = new FileProcessor();

            //Genrate road networks for NZ
            //fileProcessor.generateRoadNetwork(new List<string>() { "motorway", "motorway_link" }, "NZ_motoroway_RNG.json");
            //fileProcessor.generateRoadNetwork(new List<string>() { "trunk", "trunk_link" }, "NZ_trunk_RNG.json");
            //fileProcessor.generateRoadNetwork(new List<string>() { "ferry" }, "NZ_FW_RNG.json");

            //Genrate road networks for FJ
            //fileProcessor.generateRoadNetwork(new List<string>() { "primary", "primary_link" }, "FJ_primary_RNG.json");
            //fileProcessor.generateRoadNetwork(new List<string>() { "secondary", "secondary_link" }, "FJ_secondary_RNG.json");
            //fileProcessor.generateRoadNetwork(new List<string>() { "ferry" }, "FJ_FW_RNG.json");

            //Generate Data for NZ
            //fileProcessor.generateIncorrectMotorwayConnections("NZ_IncorrectConnections.json");
            //fileProcessor.generateDisconnectionsDataBFS(new List<string>() { "motorway", "ferry" }, "NZ_disconnections_Ferry.json");
            //fileProcessor.generateDisconnectionsDataBFS(new List<string>() { "motorway" }, "NZ_disconnections.json");

            //Generate Data for FJ
            //fileProcessor.generateIncorrectPrimaryConnectionsFiji("FJ_Incorrect_PR_Connections.json");
            //fileProcessor.generateIncorrectPrimaryConnections("FJ_Incorrect_PR_All_Connections.json");
            //fileProcessor.generateDisconnectionsDataBFS(new List<string>() { "primary", "ferry" }, "FJ_disconnections_Ferry.json");
            //fileProcessor.generateDisconnectionsDataBFS(new List<string>() { "primary" }, "FJ_disconnections.json");


            //Run BigBoi For NZ via T
            //fileProcessor.suggestConnectivityFixBasedOnLeastAmountOfNodes(new List<string>() { "motorway", "motorway_link" }, new List<string>() { "trunk", "trunk_link", "motorway_link" }, connectivityFile);
            //fileProcessor.getWaysFromNodes(connectivityFile, "NZ_T-TL-Fix_Lower.json");


            //Run BigBoi For NZ Via T and P
            //fileProcessor.suggestConnectivityFixBasedOnLeastAmountOfNodes(new List<string>() { "motorway", "motorway_link" }, new List<string>() { "trunk", "trunk_link", "motorway_link","primary","primary_link" }, connectivityFile);
            //fileProcessor.getWaysFromNodes(connectivityFile, "NZ_T-TL-P-PL-Fix.json");


            //Run BigBoi for FJ
            //fileProcessor.suggestConnectivityFixBasedOnLeastAmountOfNodes(new List<string>() { "primary", "primary_link"}, new List<string>() { "secondary", "secondary_link", "primary_link" }, connectivityFile);
            //fileProcessor.getWaysFromNodes(connectivityFile, "FJ-P_viaS.json");


            //fileProcessor.suggestConnectivityFixBasedOnLeastAmountOfNodes(new List<string>() { "primary", "primary_link" }, new List<string>() { "secondary", "secondary_link", "primary_link", "tertiary", "tertiary_link" }, connectivityFile2);
            //fileProcessor.getWaysFromNodes(connectivityFile2, "FJ-P_viaST.json");


            //fileProcessor.suggestConnectivityFixBasedOnLeastAmountOfNodes(new List<string>() { "primary", "primary_link", "secondary", "secondary_link" }, new List<string>() { "tertiary", "tertiary_link", "secondary", "secondary_link", "primary_link" }, connectivityFile1);
            //fileProcessor.getWaysFromNodes(connectivityFile1, "FJ-PS_viaST.json");

        }
    }
}
