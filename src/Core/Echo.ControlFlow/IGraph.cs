using System.Collections.Generic;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides members to model a graph-like structure.
    /// </summary>
    public interface IGraph : IGraphSegment
    {
        /// <summary>
        /// Gets a collection of all edges that transfer control from one block to the other in the graph.
        /// </summary>
        /// <returns>The edges.</returns>
        IEnumerable<IEdge> GetEdges();
    }
}