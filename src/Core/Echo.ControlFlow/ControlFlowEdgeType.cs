namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides all possible edge types that are supported in a control flow graph.
    /// </summary>
    public enum ControlFlowEdgeType
    {
        /// <summary>
        /// Indicates the edge is the default fallthrough edge of a node, and is traversed when no other edge is traversed.
        /// </summary>
        FallThrough,
        
        /// <summary>
        /// Indicates the edge is only traversed when a specific condition is met.
        /// </summary>
        Conditional,
        
        /// <summary>
        /// Indicates the edge is only traversed in abnormal circumstances, typically when an exception occurs.
        /// </summary>
        Abnormal,
        
        /// <summary>
        /// Indicates the edge is not actually a real edge, but a new node was found at the target.
        /// </summary>
        None
    }
}