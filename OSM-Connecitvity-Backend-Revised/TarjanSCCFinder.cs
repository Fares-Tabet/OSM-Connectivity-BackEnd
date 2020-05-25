using System;
using System.Collections.Generic;
using Nito.Collections;

namespace OSM_Connecitvity_Backend_Revised
{
	using System;
	using System.Collections.Generic;

	class NodeTarjan
	{
		public int LowLink { get; set; }
		public int Index { get; set; }
		public string N { get; set; }

		public NodeTarjan(string n)
		{
			N = n;
			Index = -1;
			LowLink = 0;
		}
	}

	class GraphTarjan
	{
		public HashSet<NodeTarjan> V { get; set; }
		public Dictionary<NodeTarjan, HashSet<NodeTarjan>> Adj { get; set; }

        public GraphTarjan()
        {
			this.V = new HashSet<NodeTarjan>();
			this.Adj = new Dictionary<NodeTarjan, HashSet<NodeTarjan>>();
        }

		/// <summary>
		/// Tarjan's strongly connected components algorithm
		/// </summary>
		public void Tarjan()
		{
			var index = 0; // number of nodes
			var S = new Stack<NodeTarjan>();

			Action<NodeTarjan> StrongConnect = null;
			StrongConnect = (v) =>
			{
				// Set the depth index for v to the smallest unused index
				v.Index = index;
				v.LowLink = index;

				index++;
				S.Push(v);

				// Consider successors of v
				foreach (var w in Adj[v])
					if (w.Index < 0)
					{
						// Successor w has not yet been visited; recurse on it
						StrongConnect(w);
						v.LowLink = Math.Min(v.LowLink, w.LowLink);
					}
					else if (S.Contains(w))
						// Successor w is in stack S and hence in the current SCC
						v.LowLink = Math.Min(v.LowLink, w.Index);

				// If v is a root node, pop the stack and generate an SCC
				if (v.LowLink == v.Index)
				{
					Console.Write("SCC: ");

					NodeTarjan w;
					do
					{
						w = S.Pop();
						Console.Write(w.N + " ");
					} while (w != v);

					Console.WriteLine();
				}
			};

			foreach (var v in V)
				if (v.Index < 0)
					StrongConnect(v);
		}
	}
}
