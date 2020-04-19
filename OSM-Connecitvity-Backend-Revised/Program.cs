using System;
using System.Collections.Generic;

namespace OSM_Connecitvity_Backend_Revised
{
    class Program
    {
        static void Main(string[] args)
        {
            //FileParser parser = new FileParser("NZ_allRoads.xml");
            //parser.createDataFiles();
            FileProcessor fileProcessor = new FileProcessor();
            //fileProcessor.generateRoadNetwork(new List<string>() { "trunk","trunk_link"},"NZ_trunk.json");
            fileProcessor.generateDisconnectionsDataBFS("NZ_disconnections.json");
        }
    }
}
