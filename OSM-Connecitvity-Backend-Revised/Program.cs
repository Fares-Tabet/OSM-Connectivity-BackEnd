using System;
using System.Collections.Generic;

namespace OSM_Connecitvity_Backend_Revised
{
    class Program
    {
        static void Main(string[] args)
        {
            //FileParser parser = new FileParser("NZ_allRoads_latest.xml");
            //parser.createDataFiles();


            FileProcessor fileProcessor = new FileProcessor();

            //fileProcessor.generateIncorrectMotorwayConnections("NZ_IncorrectConnections.json");

            fileProcessor.generateDisconnectionsDataBFS( new List<string>() { "motorway","motorway_link"}, "NZ_disconnections.json");
            //fileProcessor.getWaysFromNodes();

        }
    }
}
