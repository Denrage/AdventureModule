using Neo.IronLua;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Denrage.AdventureModule.Adventure
{
    public class DialogGraph
    {
        private readonly List<Node> nodes = new List<Node>();
        private readonly List<Edge> edges = new List<Edge>();
        private readonly string name;

        public event Action<AdventureDialog> OnBuilt;

        public Node InitialNode => this.nodes.First();

        public Node GetNodeById(int id)
            => this.nodes.FirstOrDefault(x => x.Id == id);

        public DialogGraph(string name)
        {
            this.name = name;
        }

        public void AddNode(int id, string text)
        {
            this.nodes.Add(new Node()
            {
                Id = id,
                Text = text,
            });
        }

        public void AddEdge(int node, int nextNode, string text, Func<bool> predicate = null, Func<LuaResult> action = null)
        {
            if (predicate is null)
            {
                predicate = () => true;
            }

            this.edges.Add(new Edge()
            {
                NextNodeId = nextNode,
                NodeId = node,
                Predicate = predicate,
                Text = text,
                Action = action,
            });
        }

        public class Node
        {
            public int Id { get; set; }

            public string Text { get; set; }

            public List<Edge> IncomingEdges { get; } = new List<Edge>();

            public List<Edge> OutgoingEdges { get; } = new List<Edge>();
        }

        public class Edge
        {
            public int NodeId { get; set; }

            public int NextNodeId { get; set; }

            public Func<bool> Predicate { get; set; }

            public Func<LuaResult> Action { get; set; }

            public string Text { get; set; }

            public Node PreviousNode { get; set; }

            public Node NextNode { get; set; }
        }

        public AdventureDialog Build()
        {
            foreach (var node in this.nodes)
            {
                foreach (var edge in this.edges)
                {
                    if (edge.NodeId == node.Id)
                    {
                        node.OutgoingEdges.Add(edge);
                        edge.PreviousNode = node;
                    }
                    else if (edge.NextNodeId == node.Id)
                    {
                        node.IncomingEdges.Add(edge);
                        edge.NextNode = node;
                    }
                }
            }

            var result = new AdventureDialog(this)
            {
                Id = ToGuid(this.name),
            };
            this.OnBuilt?.Invoke(result);
            return result;

        }

        public static Guid ToGuid(string src)
        {
            var stringbytes = System.Text.Encoding.UTF8.GetBytes(src);
            var hashedBytes = new System.Security.Cryptography
                .SHA1CryptoServiceProvider()
                .ComputeHash(stringbytes);
            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes);
        }
    }
}

