using System;
using System.Collections.Generic;

namespace Echo.Graphing.Analysis.Traversal
{
    /// <summary>
    /// Provides a mechanism for recording a post traversal order.
    /// </summary>
    public class PostOrderRecorder
    {
        private readonly Stack<INode> _stack = new();
        private readonly List<INode> _order = new();

        /// <summary>
        /// Creates a new post traversal order and hooks into the provided traversal object.
        /// </summary>
        /// <param name="traversal">The traversal object to hook into.</param>
        public PostOrderRecorder(ITraversal traversal)
        {
            traversal.NodeDiscovered += TraversalOnNodeDiscovered;
            traversal.TraversalCompleted += TraversalOnTraversalCompleted;
        }

        /// <summary>
        /// Gets the final post-order of nodes that was recorded.
        /// </summary>
        /// <returns></returns>
        public IList<INode> GetOrder()
        {
            return _order.AsReadOnly();
        }

        private void TraversalOnNodeDiscovered(object? sender, NodeDiscoveryEventArgs e)
        {
            if (!e.ContinueExploring)
                return;
            
            if (e.Origin == null)
            {
                AddRemainingNodes();
            }
            else
            {
                var originNode = e.Origin.GetOtherNode(e.NewNode);
                while (_stack.Peek() != originNode)
                    _order.Add(_stack.Pop());
            }

            _stack.Push(e.NewNode);
        }

        private void TraversalOnTraversalCompleted(object? sender, EventArgs e)
        {
            AddRemainingNodes();
        }

        private void AddRemainingNodes()
        {
            while (_stack.Count > 0)
                _order.Add(_stack.Pop());
        }
    }
}