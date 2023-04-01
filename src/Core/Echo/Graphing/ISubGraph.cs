using System.Collections.Generic;

namespace Echo.Graphing
{
    /// <summary>
    /// Represents a region of a graph, comprising of a subset of nodes of the full graph.
    /// </summary>
    public interface ISubGraph
    {
        /// <summary>
        /// Gets a collection of nodes that this segment contains.
        /// </summary>
        /// <returns>The nodes.</returns>
        IEnumerable<INode> GetNodes();

        /// <summary>
        /// Gets a collection of sub graphs that this segment contains (if any).
        /// </summary>
        /// <returns>The sub graphs.</returns>
        IEnumerable<ISubGraph> GetSubGraphs();
    }
}