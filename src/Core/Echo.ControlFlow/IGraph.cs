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

        /// <summary>
        /// Gets an ordered collection of all exception handlers that are defined in the control flow graph.
        /// </summary>
        /// <returns>The exception handlers.</returns>
        /// <remarks>
        /// If two exception handlers are nested (i.e. they overlap in the try segments), the one that occurs first in
        /// this collection is the enclosing exception handler.
        /// </remarks>
        IEnumerable<IExceptionHandler> GetExceptionHandlers();
    }
}