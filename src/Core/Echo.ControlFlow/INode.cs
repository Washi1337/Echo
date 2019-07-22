using System.Collections.Generic;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Represents a single node in a generic control flow graph, which can be connected
    /// to other nodes in various ways.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Gets the default edge that is taken after executing the node. 
        /// </summary>
        /// <returns>The default fallthrough edge.</returns>
        IEdge GetFallThroughEdge();

        /// <summary>
        /// Gets a collection of edges that are only taken when a certain condition is met.
        /// </summary>
        /// <returns>The conditional edges.</returns>
        IEnumerable<IEdge> GetConditionalEdges();

        /// <summary>
        /// Gets a collection of edges that are only taken when an abnormal execution is enforced (typically through
        /// exception handlers).
        /// </summary>
        /// <returns>The abnormal edges.</returns>
        IEnumerable<IEdge> GetAbnormalEdges();

        /// <summary>
        /// Gets a collection of all edges that target this node.
        /// </summary>
        /// <returns>The incoming edges.</returns>
        IEnumerable<IEdge> GetIncomingEdges();

        /// <summary>
        /// Gets a collection of all outgoing edges originating from this node.
        /// </summary>
        /// <returns>The outgoing edges.</returns>
        IEnumerable<IEdge> GetOutgoingEdges();

        /// <summary>
        /// Gets a collection of nodes that precede this node. This includes any node that might transfer control to
        /// node this node in the complete control flow graph, regardless of edge type. 
        /// </summary>
        /// <returns>The predecessor nodes.</returns>
        IEnumerable<INode> GetPredecessors();

        /// <summary>
        /// Gets a collection of nodes that might be executed after this node. This includes any node that this node
        /// might transfer control to, regardless of edge type.
        /// </summary>
        /// <returns>The successor nodes.</returns>
        IEnumerable<INode> GetSuccessors();
    }
}