using System.Collections.Generic;

namespace Echo.Graphing.Serialization.Dot
{
    /// <summary>
    /// Provides members for adorning an edge in a graph.
    /// </summary>
    public interface IDotEdgeAdorner
    {
        /// <summary>
        /// Obtains the adornments that should be added to the edge. 
        /// </summary>
        /// <param name="edge">The edge to adorn.</param>
        /// <param name="sourceId">The identifier assigned to the source node.</param>
        /// <param name="targetId">The identifier assigned to the target node.</param>
        /// <returns>The adornments.</returns>
        IDictionary<string, string>? GetEdgeAttributes(IEdge edge, long sourceId, long targetId);
    }
}