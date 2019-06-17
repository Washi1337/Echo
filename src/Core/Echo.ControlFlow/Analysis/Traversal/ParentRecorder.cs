using System;
using System.Collections.Generic;

namespace Echo.ControlFlow.Analysis.Traversal
{
    public class ParentRecorder
    {
        private readonly IDictionary<INode, IEdge> _parents = new Dictionary<INode, IEdge>();
        private readonly ITraversal _traversal;

        public ParentRecorder(ITraversal traversal)
        {
            _traversal = traversal ?? throw new ArgumentNullException(nameof(traversal));
            traversal.NodeDiscovered += (sender, args) =>
            {
                if (!_parents.ContainsKey(args.NewNode))
                    _parents[args.NewNode] = args.Origin;
            };
        }

        public IEdge GetParentEdge(INode node)
        {
            return _parents[node];
        }

        public INode GetParent(INode node)
        {
            return _parents[node].Origin;
        }
    }
}