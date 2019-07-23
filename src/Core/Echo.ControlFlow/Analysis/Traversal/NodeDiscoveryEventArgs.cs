namespace Echo.ControlFlow.Analysis.Traversal
{
    /// <summary>
    /// Provides information about a node discovery during a traversal of a graph.
    /// </summary>
    public class NodeDiscoveryEventArgs : DiscoveryEventArgs
    {
        /// <summary>
        /// Creates a new node discovery event.
        /// </summary>
        /// <param name="newNode">The node that was discovered.</param>
        /// <param name="origin">The edge that was traversed to discover the node.</param>
        public NodeDiscoveryEventArgs(INode newNode, IEdge origin)
        {
            NewNode = newNode;
            Origin = origin;
        }
        
        /// <summary>
        /// Gets the node that was discovered.
        /// </summary>
        public INode NewNode
        {
            get;
        }

        /// <summary>
        /// Gets the edge that was traversed that resulted in the node to be discovered.
        /// </summary>
        public IEdge Origin
        {
            get;
        }
    }
}