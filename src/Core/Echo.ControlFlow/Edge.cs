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
    public class Edge<TContents> : IEdge
    {
        /// <summary>
        /// Creates a new fallthrough edge between two nodes.
        /// </summary>
        /// <param name="origin">The node to start the edge at.</param>
        /// <param name="target">The node to use as destination for the edge.</param>
        public Edge(Node<TContents> origin, Node<TContents> target)
            : this(origin, target, EdgeType.FallThrough)
        {
        }
        
        /// <summary>
        /// Creates a new edge between two nodes.
        /// </summary>
        /// <param name="origin">The node to start the edge at.</param>
        /// <param name="target">The node to use as destination for the edge.</param>
        /// <param name="edgeType">The type of the edge to create.</param>
        public Edge(Node<TContents> origin, Node<TContents> target, EdgeType edgeType)
        {
            Origin = origin;
            Target = target;
            Type = edgeType;
        }

        /// <summary>
        /// Gets the graph that contains this edge.
        /// </summary>
        public Graph<TContents> ParentGraph => Origin?.ParentGraph ?? Target?.ParentGraph;

        /// <summary>
        /// Gets the node that this edge originates from.
        /// </summary>
        public Node<TContents> Origin
        {
            get;
        }

        /// <summary>
        /// Gets the node that this edge targets.
        /// </summary>
        public Node<TContents> Target
        {
            get;
        }

        INode IEdge.Origin => Origin;

        INode IEdge.Target => Target;

        /// <summary>
        /// Gets the type of the edge.
        /// </summary>
        public EdgeType Type
        {
            get;
        }
    }
}