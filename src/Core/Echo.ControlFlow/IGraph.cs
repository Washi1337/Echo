using System.Collections.Generic;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Represents a control flow graph encoding all possible execution paths in a chunk of code.
    /// </summary>
    public interface IGraph
    {
        /// <summary>
        /// Gets the node that is executed first in the control flow graph.
        /// </summary>
        INode Entrypoint
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of all basic blocks present in the graph.
        /// </summary>
        /// <returns>The nodes.</returns>
        IEnumerable<INode> GetNodes();

        /// <summary>
        /// Gets a collection of all edges that transfer control from one block to the other in the graph.
        /// </summary>
        /// <returns>The edges.</returns>
        IEnumerable<IEdge> GetEdges();
    }
}