using System;
using System.Collections.Generic;

namespace Echo.ControlFlow.Analysis.Traversal
{
    /// <summary>
    /// Represents a depth-first node traversal of a graph.
    /// </summary>
    public class DepthFirstTraversal : ITraversal
    {
        /// <inheritdoc />
        public event EventHandler<NodeDiscoveryEventArgs> NodeDiscovered;

        public event EventHandler TraversalCompleted;

        /// <inheritdoc />
        public void Run(INode entrypoint)
        {
            if (entrypoint == null)
                throw new ArgumentNullException(nameof(entrypoint));
            
            var visited = new HashSet<INode>();
            var stack = new Stack<(INode node, IEdge edge)>();
            stack.Push((entrypoint, null));
            
            while (stack.Count > 0)
            {
                var (node, origin) = stack.Pop();
                var eventArgs = new NodeDiscoveryEventArgs(node, origin)
                {
                    ContinueExploring = visited.Add(node)
                };
                OnNodeDiscovered(eventArgs);

                if (eventArgs.Abort)
                    return;
                
                if (eventArgs.ContinueExploring)
                {
                    foreach (var edge in node.GetOutgoingEdges())
                        stack.Push((edge.Target, edge));
                }
            }
            
            OnTraversalCompleted();
        }

        protected virtual void OnNodeDiscovered(NodeDiscoveryEventArgs e)
        {
            NodeDiscovered?.Invoke(this, e);
        }

        protected virtual void OnTraversalCompleted()
        {
            TraversalCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}