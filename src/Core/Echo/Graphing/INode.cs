using System.Collections.Generic;

namespace Echo.Graphing
{
    /// <summary>
    /// Represents a single node in a generic directed graph.
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Gets a value indicating the number of incoming edges that this node is incident to.
        /// </summary>
        int InDegree
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating the number of outgoing edges that this node is incident to.
        /// </summary>
        int OutDegree
        {
            get;
        }
        
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
        /// Gets a collection of nodes that precede this node.
        /// </summary>
        /// <returns>The predecessor nodes.</returns>
        IEnumerable<INode> GetPredecessors();

        /// <summary>
        /// Gets a collection of nodes that can be reached from this node by following one of the incident edges.
        /// </summary>
        /// <returns>The successor nodes.</returns>
        IEnumerable<INode> GetSuccessors();

        /// <summary>
        /// Determines whether the provided node precedes the current node. 
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <returns><c>True</c> if the node is a predecessor, <c>false</c> otherwise.</returns>
        bool HasPredecessor(INode node);

        /// <summary>
        /// Determines whether the provided node can be reached from this node by following one of the incident edges.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <returns><c>True</c> if the node is a successor, <c>false</c> otherwise.</returns>
        bool HasSuccessor(INode node);
    }
}