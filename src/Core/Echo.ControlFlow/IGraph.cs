using System.Collections.Generic;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Represents a control flow graph encoding all possible execution paths in a chunk of code.
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