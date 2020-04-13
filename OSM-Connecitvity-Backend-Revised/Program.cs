using System;
using System.Collections.Generic;

namespace OSM_Connecitvity_Backend_Revised
{
    class Program
    {
        static void Main(string[] args)
        {
            FileParser parser = new FileParser("sydney_allRoads.xml");
            parser.createDataFiles();
            FileProcessor fileProcessor = new FileProcessor();
            fileProcessor.generateRoadNetwork(new List<string>() { "motorway","motorway_link"},"motorway.json");
            fileProcessor.generateDisconnectionsData("disconnections.json");

        }
    }
}
