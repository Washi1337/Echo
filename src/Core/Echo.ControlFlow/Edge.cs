using Echo.Core.Code;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides an implementation for a single edge in a control flow graph, including the source and target node,
    /// and the type of edge.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that the connected nodes store.</typeparam>
    public class Edge<TInstruction> : IEdge
        where TInstruction : IInstruction
    {
        /// <summary>
        /// Creates a new fallthrough edge between two nodes.
        /// </summary>
        /// <param name="source">The node to start the edge at.</param>
        /// <param name="target">The node to use as destination for the edge.</param>
        public Edge(Node<TInstruction> source, Node<TInstruction> target)
            : this(source, target, EdgeType.FallThrough)
        {
        }
        
        /// <summary>
        /// Creates a new edge between two nodes.
        /// </summary>
        /// <param name="source">The node to start the edge at.</param>
        /// <param name="target">The node to use as destination for the edge.</param>
        /// <param name="edgeType">The type of the edge to create.</param>
        public Edge(Node<TInstruction> source, Node<TInstruction> target, EdgeType edgeType)
        {
            Source = source;
            Target = target;
            Type = edgeType;
        }

        /// <summary>
        /// Gets the graph that contains this edge.
        /// </summary>
        public Graph<TInstruction> ParentGraph => Source?.ParentGraph ?? Target?.ParentGraph;

        /// <summary>
        /// Gets the node that this edge originates from.
        /// </summary>
        public Node<TInstruction> Source
        {
            get;
        }

        /// <summary>
        /// Gets the node that this edge targets.
        /// </summary>
        public Node<TInstruction> Target
        {
            get;
        }

        INode IEdge.Source => Source;

        INode IEdge.Target => Target;

        /// <inheritdoc />
        public EdgeType Type
        {
            get;
        }
    }
}