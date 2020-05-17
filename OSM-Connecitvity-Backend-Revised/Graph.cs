using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OSM_Connecitvity_Backend_Revised
{
    public class Graph
    {
        public int V;   // No. of vertices 
        public LinkedList<int>[] adj; //Adjacency List 
        FileProcessor fp = new FileProcessor();
        //Constructor 
        public Graph(int v)
        {
            V = v;
            adj = new LinkedList<int>[v];
            for (int i = 0; i < v; ++i)
                adj[i] = new LinkedList<int>();
        }

        //Function to add an edge into the graph 
        public void addEdge(int v, int w) {
            adj[v].AddLast(w);
        }

        // A recursive function to print DFS starting from v 
        public void DFSUtil(int v, Boolean[] visited)
        {
            // Mark the current node as visited and print it 
            visited[v] = true;
            Console.Write(v + " ");

            int n;

            // Recur for all the vertices adjacent to this vertex
            foreach (int i in adj[v])
            {
                n = i;
                if (!visited[n])
                    DFSUtil(n, visited);
            }
           
        }

        // Function that returns reverse (or transpose) of this graph 
        public Graph getTranspose()
        {
            Graph g = new Graph(V);
            for (int v = 0; v < V; v++)
            {
                // Recur for all the vertices adjacent to this vertex
                foreach(int i in adj[v])
                {
                    g.adj[i].AddLast(v);
                }
               
            }
            return g;
        }

        public void fillOrder(int v, Boolean[] visited, Stack stack)
        {
            // Mark the current node as visited and print it 
            visited[v] = true;

            // Recur for all the vertices adjacent to this vertex 
            foreach (int i in adj[v]) 
            {
                int n = i;
                if (!visited[n])
                    fillOrder(n, visited, stack);
            }

            // All vertices reachable from v are processed by now, 
            // push v to Stack 
            stack.Push(v);
        }
    }
}
