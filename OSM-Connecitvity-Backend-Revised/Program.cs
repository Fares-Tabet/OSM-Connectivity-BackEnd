using System;

namespace OSM_Connecitvity_Backend_Revised
{
    class Program
    {
        static void Main(string[] args)
        {
            FileParser parser = new FileParser("C:\\Users\\Fares\\Desktop\\sydney_allRoads.xml");
            parser.createDataFiles();
        }
    }
}
