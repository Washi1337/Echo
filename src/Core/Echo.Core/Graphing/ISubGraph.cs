using System.Collections.Generic;

namespace Echo.Core.Graphing
{
    /// <summary>
    /// Represents a region of a graph, comprising of a subset of nodes of the full graph.
    /// </summary>
    public interface ISubGraph
    {
        /// <summary>
        /// Obtains a node by its unique identifier.
        /// </summary>
        /// <param name="id">The node identifier.</param>
        /// <returns>The node, or <c>null</c> if no such node was found.</returns>
        INode GetNodeById(long id);
        
        /// <summary>
        /// Gets a collection of nodes that this segment contains.
        /// </summary>
        /// <returns>The nodes.</returns>
        IEnumerable<INode> GetNodes();
    }
}