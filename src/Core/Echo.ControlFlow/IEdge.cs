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
        INode Origin
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
    
    /// <summary>
    /// Provides utility methods that further extend the graph model classes.
    /// </summary>
    public static partial class GraphExtensions
    {
        /// <summary>
        /// Given an edge and one of the nodes that this edge connects with, gets the other end of the edge.
        /// </summary>
        /// <param name="edge">The edge.</param>
        /// <param name="node">One of the nodes of the edge.</param>
        /// <returns>The other end of the edge.</returns>
        public static INode GetOtherNode(this IEdge edge, INode node)
        {
            return edge.Origin == node ? edge.Target : edge.Origin;
        }   
    }
}