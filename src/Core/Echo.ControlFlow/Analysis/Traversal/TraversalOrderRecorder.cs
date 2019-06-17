using System;
using System.Collections.Generic;

namespace Echo.ControlFlow.Analysis.Traversal
{
    public class TraversalOrderRecorder
    {
        private readonly IDictionary<INode, int> _indices = new Dictionary<INode, int>();
        private readonly List<INode> _order = new List<INode>();
        private readonly ITraversal _traversal;

        public TraversalOrderRecorder(ITraversal traversal)
        {
            _traversal = traversal ?? throw new ArgumentNullException(nameof(traversal));
            _traversal.NodeDiscovered += TraversalOnNodeDiscovered;
        }
        
        /// <summary>
        /// Gets a collection of all the nodes that were discovered during the traversal.
        /// </summary>
        public ICollection<INode> TraversedNodes => _indices.Keys;

        /// <summary>
        /// Gets the index of the node during the traversal. 
        /// </summary>
        /// <param name="node">The node to get the index from.</param>
        /// <returns>The index.</returns>
        public int GetIndex(INode node)
        {
            return _indices.TryGetValue(node, out int index) ? index : -1;
        }

        public IList<INode> GetTraversal()
        {
            return _order.AsReadOnly();
        }
        
        private void TraversalOnNodeDiscovered(object sender, NodeDiscoveryEventArgs e)
        {
            if (!_indices.ContainsKey(e.NewNode))
            {
                _indices[e.NewNode] = _indices.Count;
                _order.Add(e.NewNode);
            }
        }
    }
}