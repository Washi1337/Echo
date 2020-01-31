using System;
using System.Collections.Generic;
using Echo.Core.Graphing;

namespace Echo.Core.Graphing.Analysis.Traversal
{
    /// <summary>
    /// Represents a depth-first node traversal of a graph.
    /// </summary>
    public class DepthFirstTraversal : ITraversal
    {
        /// <inheritdoc />
        public event EventHandler<NodeDiscoveryEventArgs> NodeDiscovered;
        
        /// <inheritdoc />
        public event EventHandler TraversalCompleted;

        /// <summary>
        /// Creates a new depth first traversal.
        /// </summary>
        public DepthFirstTraversal()
            : this(false)
        {
        }

        /// <summary>
        /// Creates a new depth first traversal.
        /// </summary>
        /// <param name="reverseTraversal">
        /// <c>True</c> if the traversal should traverse the graph in a reversed manner.
        /// That is, whether the traversal should treat each edge as if it was reversed.
        /// </param>
        public DepthFirstTraversal(bool reverseTraversal)
        {
            ReverseTraversal = reverseTraversal;
        }

        /// <summary>
        /// Gets a value indicating the traversal algorithm should traverse either outgoing or incoming edges. 
        /// </summary>
        public bool ReverseTraversal
        {
            get;
        }

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
                    var nextEdges = ReverseTraversal ? node.GetIncomingEdges() : node.GetOutgoingEdges();
                    foreach (var edge in nextEdges)
                        stack.Push((edge.GetOtherNode(node), edge));
                }
            }
            
            OnTraversalCompleted();
        }

        /// <summary>
        /// Fires and handles the node discovered event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected virtual void OnNodeDiscovered(NodeDiscoveryEventArgs e)
        {
            NodeDiscovered?.Invoke(this, e);
        }

        /// <summary>
        /// Fires and handles the traversal completed event.
        /// </summary>
        protected virtual void OnTraversalCompleted()
        {
            TraversalCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}