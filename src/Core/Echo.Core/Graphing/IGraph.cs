using System.Collections.Generic;

namespace Echo.Core.Graphing
{
    /// <summary>
    /// Provides members to model a directed graph-like structure.
    /// </summary>
    public interface IGraph : ISubGraph
    {
        /// <summary>
        /// Gets a collection of all directed edges (or arcs) that connect nodes in the directed graph.
        /// </summary>
        /// <returns>The edges.</returns>
        IEnumerable<IEdge> GetEdges();
    }
}