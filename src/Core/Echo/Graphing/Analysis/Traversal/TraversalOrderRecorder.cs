using System;
using System.Collections.Generic;

namespace Echo.Graphing.Analysis.Traversal
{
    /// <summary>
    /// Provides a mechanism to record the order in which each node in the graph was traversed by a traversal algorithm.
    /// </summary>
    public class TraversalOrderRecorder
    {
        private readonly Dictionary<INode, int> _indices = new();
        private readonly List<INode> _order = new();

        /// <summary>
        /// Creates a new traversal order recorder and hooks into the provided traversal object.
        /// </summary>
        /// <param name="traversal">The traversal object to hook into.</param>
        public TraversalOrderRecorder(ITraversal traversal)
        {
            if (traversal == null)
                throw new ArgumentNullException(nameof(traversal));
            traversal.NodeDiscovered += TraversalOnNodeDiscovered;
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

        /// <summary>
        /// Gets the full traversal as an ordered list of nodes.
        /// </summary>
        /// <returns>The traversal.</returns>
        public IList<INode> GetTraversal()
        {
            return _order.AsReadOnly();
        }
        
        private void TraversalOnNodeDiscovered(object? sender, NodeDiscoveryEventArgs e)
        {
            if (!_indices.ContainsKey(e.NewNode))
            {
                _indices[e.NewNode] = _indices.Count;
                _order.Add(e.NewNode);
            }
        }
    }
}