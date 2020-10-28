using Echo.Core.Code;
using Echo.Core.Graphing;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides an implementation for a single edge in a control flow graph, including the source and target node,
    /// and the type of edge.
    /// </summary>
    /// <remarks>
    /// If an edge is in between two nodes, it means that control might be transferred from the one node to the other
    /// during the execution of the program that is encoded by the control flow graph. 
    /// </remarks>
    /// <typeparam name="TContents">The type of contents that the connected nodes store.</typeparam>
    public class ControlFlowEdge<TContents> : IEdge
    {
        /// <summary>
        /// Creates a new fallthrough edge between two nodes.
        /// </summary>
        /// <param name="origin">The node to start the edge at.</param>
        /// <param name="target">The node to use as destination for the edge.</param>
        public ControlFlowEdge(ControlFlowNode<TContents> origin, ControlFlowNode<TContents> target)
            : this(origin, target, ControlFlowEdgeType.FallThrough)
        {
        }
        
        /// <summary>
        /// Creates a new edge between two nodes.
        /// </summary>
        /// <param name="origin">The node to start the edge at.</param>
        /// <param name="target">The node to use as destination for the edge.</param>
        /// <param name="edgeType">The type of the edge to create.</param>
        public ControlFlowEdge(ControlFlowNode<TContents> origin, ControlFlowNode<TContents> target, ControlFlowEdgeType edgeType)
        {
            Origin = origin;
            Target = target;
            Type = edgeType;
        }

        /// <summary>
        /// Gets the graph that contains this edge.
        /// </summary>
        public ControlFlowGraph<TContents> ParentGraph => Origin?.ParentGraph ?? Target?.ParentGraph;

        /// <summary>
        /// Gets the node that this edge originates from.
        /// </summary>
        public ControlFlowNode<TContents> Origin
        {
            get;
        }

        /// <summary>
        /// Gets the node that this edge targets.
        /// </summary>
        public ControlFlowNode<TContents> Target
        {
            get;
        }

        INode IEdge.Origin => Origin;

        INode IEdge.Target => Target;

        /// <summary>
        /// Gets the type of the edge.
        /// </summary>
        public ControlFlowEdgeType Type
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Origin.Offset:X8} -> {Target.Offset:X8} ({Type})";
    }
}