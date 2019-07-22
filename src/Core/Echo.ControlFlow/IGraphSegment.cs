using System.Collections.Generic;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Represents a segment in a control flow graph that has a single entrypoint node.
    /// </summary>
    public interface IGraphSegment
    {
        /// <summary>
        /// Gets the node that is executed first when executing the segment.
        /// </summary>
        INode Entrypoint
        {
            get;
        }

        /// <summary>
        /// Gets a collection of nodes that this segment contains.
        /// </summary>
        /// <returns>The nodes.</returns>
        IEnumerable<INode> GetNodes();
    }
}