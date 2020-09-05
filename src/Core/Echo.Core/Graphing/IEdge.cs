namespace Echo.Core.Graphing
{
    /// <summary>
    /// Represents a single edge that connects two nodes together in a directed graph.
    /// </summary>
    public interface IEdge
    {
        /// <summary>
        /// Gets the node that this edge starts at in the directed graph.
        /// </summary>
        INode Origin
        {
            get;
        }

        /// <summary>
        /// Gets the node that this edge points to in the directed graph.
        /// </summary>
        INode Target
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