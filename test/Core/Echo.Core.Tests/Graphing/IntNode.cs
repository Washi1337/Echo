using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Construction;
using Echo.Core.Graphing;

namespace Echo.Core.Tests.Graphing
{
    public class IntNode : INode
    {
        private IList<IEdge> _incomingEdges = new List<IEdge>();
        private IList<IEdge> _outgoingEdges = new List<IEdge>();
        
        public IntNode(long id)
        {
            Id = id;
        }
        
        public long Id
        {
            get;
        }

        public void ConnectWith(IntNode node)
        {
            var edge = new Edge(this, node);
            _outgoingEdges.Add(edge);
            node._incomingEdges.Add(edge);
        }
        
        public IEnumerable<IEdge> GetIncomingEdges() => _incomingEdges;

        public IEnumerable<IEdge> GetOutgoingEdges() => _outgoingEdges;

        public IEnumerable<INode> GetPredecessors() => _incomingEdges.Select(e => e.Origin);

        public IEnumerable<INode> GetSuccessors() => _outgoingEdges.Select(e => e.Target);

        public bool HasPredecessor(INode node) => GetPredecessors().Contains(node);

        public bool HasSuccessor(INode node) => GetSuccessors().Contains(node);
    }
}