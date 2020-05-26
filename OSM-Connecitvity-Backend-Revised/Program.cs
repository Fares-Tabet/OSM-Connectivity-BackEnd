using System;
using System.Collections.Generic;

namespace OSM_Connecitvity_Backend_Revised
{
    class Program
    {
        static void Main(string[] args)
        {
            // FileParser parser = new FileParser("/Users/fares/Downloads/NZ_allRoads.xml");
            //parser.createDataFiles();


            FileProcessor fileProcessor = new FileProcessor();

            //fileProcessor.generateIncorrectMotorwayConnections("NZ_IncorrectConnections.json");

            fileProcessor.generateDisconnectionsDataBFS( new List<string>() { "motorway","motorway_link"}, "NZ_disconnections.json");
            //fileProcessor.getWaysFromNodes();

            string x = "3029123087 6138484213 3029123116 3029123069 947891767 947891928 2915526575 2915526590 2915526581 4846868447 4464231993 4846868471 4846868470 2915526593 2915527214 4464231988 4464231978 4464229417 4711111451 4464229409 4464229430 4711121985 4711121984 4691997458 4691997443 4566484301 2915567602 722121199 6027069395 2915567563 4691997480 4691997460 4711129190 4711129191 2915567504 2915565892 4711111442 4711111441 4464231981 4464231979 2915526562 2915526576 4846868457 4846868453 4464231987 4464231986 4469562794 4469562796 4469562793 3029122823 200752179 3029123107";
            Console.WriteLine("total nodes " + x.Split(" ").Length.ToString());

            

        }
    }
}
