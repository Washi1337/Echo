namespace Echo.ControlFlow
{
    /// <summary>
    /// Represents a single edge that connects to nodes together in a control flow graph.
    /// </summary>
    /// <remarks>
    /// If an edge is in between two nodes, it means that control might be transferred from the one node to the other
    /// during the execution of the program that is encoded by the control flow graph. 
    /// </remarks>
    public interface IEdge
    {
        /// <summary>
        /// Gets the node that this edge starts at.
        /// </summary>
        INode Source
        {
            get;
        }

        /// <summary>
        /// Gets the node that this edge transfers control to when followed.
        /// </summary>
        INode Target
        {
            get;
        }

        /// <summary>
        /// Gets the type of the edge.
        /// </summary>
        EdgeType Type
        {
            get;
        }
    }
}