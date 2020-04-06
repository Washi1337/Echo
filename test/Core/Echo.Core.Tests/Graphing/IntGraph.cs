using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;

namespace Echo.Core.Tests.Graphing
{
    public class IntGraph : IGraph
    {
        private readonly IDictionary<long, INode> _nodes = new Dictionary<long, INode>();
        
        public IntNode AddNode(int x)
        {
            var node = new IntNode(x);
            _nodes.Add(x, node);
            return node;
        }
        
        public INode GetNodeById(long id) => _nodes[id];

        public IEnumerable<INode> GetNodes() => _nodes.Values;

        public IEnumerable<IEdge> GetEdges() => _nodes.SelectMany(n => n.Value.GetOutgoingEdges());

        public IEnumerable<ISubGraph> GetSubGraphs() => Enumerable.Empty<ISubGraph>();
    }
}