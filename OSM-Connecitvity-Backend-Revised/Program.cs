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

            //GraphTarjan g = new GraphTarjan();
            //NodeTarjan node0 = new NodeTarjan(0 + "");
            //NodeTarjan node1 = new NodeTarjan(1 + "");
            //NodeTarjan node2 = new NodeTarjan(2 + "");
            //NodeTarjan node3 = new NodeTarjan(3234213 + "");
            //NodeTarjan node4 = new NodeTarjan(4 + "");
            //NodeTarjan node5 = new NodeTarjan(5 + "");
            //NodeTarjan node6 = new NodeTarjan(6 + "");
            //NodeTarjan node7 = new NodeTarjan(7123432 + "");

            //g.V.Add(node0);
            //g.V.Add(node1);
            //g.V.Add(node2);
            //g.V.Add(node3);
            //g.V.Add(node4);
            //g.V.Add(node5);
            ////g.V.Add(node6);
            //g.V.Add(node7);



            //g.Adj.Add(node0, new HashSet<NodeTarjan>() { node1 });
            //g.Adj.Add(node1, new HashSet<NodeTarjan>() { node2 });
            //g.Adj.Add(node2, new HashSet<NodeTarjan>() { node0 });
            //g.Adj.Add(node3, new HashSet<NodeTarjan>() { node4, node7 });
            //g.Adj.Add(node4, new HashSet<NodeTarjan>() { node5 });
            //g.Adj.Add(node5, new HashSet<NodeTarjan>() { node0, node6 });
            //g.Adj.Add(node6, new HashSet<NodeTarjan>() { node2, node0, node4 });
            //g.Adj.Add(node7, new HashSet<NodeTarjan>() { node3 });

            //g.Tarjan();

        }
    }
}
