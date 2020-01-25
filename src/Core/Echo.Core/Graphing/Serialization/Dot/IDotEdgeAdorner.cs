using System.Collections.Generic;

namespace Echo.Core.Graphing.Serialization.Dot
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
        /// <returns>The adornments.</returns>
        IDictionary<string, string> GetEdgeAttributes(IEdge edge);
    }
}