using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Specialized;
using Echo.Core.Code;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides a generic base implementation for a node present in a graph, containing a user
    /// defined object in a type-safe manner.
    /// </summary>
    /// <typeparam name="TContents">The type of data to store in the node.</typeparam>
    public class Node<TContents> : INode
    {
        private Edge<TContents> _fallThroughEdge;

        /// <summary>
        /// Creates a new basic block filled with the provided data, to be added to the graph.
        /// </summary>
        /// <param name="contents">The data to store in the node.</param>
        public Node(TContents contents)
        {
            Contents = contents;
            ConditionalEdges = new AdjacencyCollection<TContents>(this, EdgeType.Conditional);
            AbnormalEdges = new AdjacencyCollection<TContents>(this, EdgeType.Abnormal);
        }

        /// <summary>
        /// Gets the graph that contains this node, or <c>null</c> if the node is not added to any graph yet.  
        /// </summary>
        public Graph<TContents> ParentGraph
        {
            get;
            internal set;
        }

        public TContents Contents
        {
            get;
        }

        /// <summary>
        /// Gets or sets the neighbour to which the control is transferred to after execution of this block and no
        /// other condition is met.
        /// </summary>
        public Node<TContents> FallThroughNeighbour
        {
            get => FallThroughEdge?.Target;
            set => FallThroughEdge = new Edge<TContents>(this, value);
        }

        /// <summary>
        /// Gets or sets the edge to the neighbour to which the control is transferred to after execution of this block
        /// and no other condition is met.
        /// </summary>
        public Edge<TContents> FallThroughEdge
        {
            get => _fallThroughEdge;
            set
            {
                _fallThroughEdge?.Target.IncomingEdges.Remove(value);

                if (value != null)
                {
                    AdjacencyCollection<TContents>.AssertEdgeValidity(this, EdgeType.FallThrough, value);
                    value.Target.IncomingEdges.Add(value);
                }

                _fallThroughEdge = value;
            }
        }

        /// <summary>
        /// Gets a collection of conditional edges that originate from this source.
        /// </summary>
        /// <remarks>
        /// These edges are typically present when a node is a basic block encoding the header of an if statement
        /// or a loop. 
        /// </remarks>
        public AdjacencyCollection<TContents> ConditionalEdges
        {
            get;
        }

        /// <summary>
        /// Gets a collection of abnormal edges that originate from this source.
        /// </summary>
        /// <remarks>
        /// These edges are typically present when a node is part of a region of code protected by an exception handler.
        /// </remarks>
        public AdjacencyCollection<TContents> AbnormalEdges
        {
            get;
        }
        
        /// <summary>
        /// Provides a record of all incoming edges.
        /// </summary>
        /// <remarks>
        /// This property is automatically updated by the adjacency lists and the fall through edge property associated
        /// to all nodes that might connect themselves to the current node. Do not change it in this class.
        /// </remarks>
        internal ICollection<Edge<TContents>> IncomingEdges
        {
            get;
        } = new List<Edge<TContents>>();

        /// <summary>
        /// Connects the node to the provided neighbour using a fallthrough edge. 
        /// </summary>
        /// <param name="neighbour">The node to connect to.</param>
        /// <returns>The edge that was used to connect the two nodes together.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="neighbour"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Occurs when the node already contains a fallthrough edge to another node.</exception>
        public Edge<TContents> ConnectWith(Node<TContents> neighbour)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            return ConnectWith(neighbour, EdgeType.FallThrough);
        }
        
        /// <summary>
        /// Connects the node to the provided neighbour. 
        /// </summary>
        /// <param name="neighbour">The node to connect to.</param>
        /// <param name="edgeType">The type of edge to use for connecting to the other node.</param>
        /// <returns>The edge that was used to connect the two nodes together.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="neighbour"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Occurs when <paramref name="edgeType"/> equals <see cref="EdgeType.FallThrough"/>, and the node
        ///     already contains a fallthrough edge to another node.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when an invalid edge type was provided.</exception>
        public Edge<TContents> ConnectWith(Node<TContents> neighbour, EdgeType edgeType)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            var edge = new Edge<TContents>(this, neighbour, edgeType);
            
            switch (edgeType)
            {
                case EdgeType.FallThrough:
                    if (FallThroughEdge != null)
                        throw new InvalidOperationException("Node already has a fallthrough edge to another node.");
                    FallThroughEdge = edge;
                    break;

                case EdgeType.Conditional:
                    ConditionalEdges.Add(edge);
                    break;
                
                case EdgeType.Abnormal:
                    AbnormalEdges.Add(edge);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(edgeType), edgeType, null);
            }

            return edge;
        }

        /// <summary>
        /// Gets a collection of all edges that target this node.
        /// </summary>
        /// <returns>The incoming edges.</returns>
        public IEnumerable<Edge<TContents>> GetIncomingEdges()
        {
            return IncomingEdges;
        }
        
        /// <summary>
        /// Gets a collection of all outgoing edges originating from this node.
        /// </summary>
        /// <returns>The outgoing edges.</returns>
        public IEnumerable<Edge<TContents>> GetOutgoingEdges()
        {
            var result = new List<Edge<TContents>>();
            if (FallThroughEdge != null)
                result.Add(FallThroughEdge);
            result.AddRange(ConditionalEdges);
            result.AddRange(AbnormalEdges);
            return result;
        }

        /// <summary>
        /// Gets a collection of nodes that precede this node. This includes any node that might transfer control to
        /// node this node in the complete control flow graph, regardless of edge type. 
        /// </summary>
        /// <returns>The predecessor nodes.</returns>
        public IEnumerable<Node<TContents>> GetPredecessors()
        {
            return GetIncomingEdges()
                .Select(n => n.Origin)
                .Distinct();
        }

        /// <summary>
        /// Gets a collection of nodes that might be executed after this node. This includes any node that this node
        /// might transfer control to, regardless of edge type.
        /// </summary>
        /// <returns>The successor nodes.</returns>
        public IEnumerable<Node<TContents>> GetSuccessors()
        {
            return GetOutgoingEdges()
                .Select(n => n.Target)
                .Distinct();
        }

        /// <summary>
        /// Determines whether another node is a successor of this node.
        /// </summary>
        /// <param name="neighbour">The potential successor.</param>
        /// <returns><c>True</c> if the provided node is a successor, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Occurs when the provided successor is <c>null</c></exception>
        public bool HasSuccessor(Node<TContents> neighbour)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            return FallThroughNeighbour == neighbour
                   || ConditionalEdges.Contains(neighbour)
                   || AbnormalEdges.Contains(neighbour);
        }

        IEdge INode.GetFallThroughEdge() => FallThroughEdge;

        IEnumerable<IEdge> INode.GetConditionalEdges() => ConditionalEdges;

        IEnumerable<IEdge> INode.GetAbnormalEdges() => AbnormalEdges;

        IEnumerable<IEdge> INode.GetIncomingEdges() => GetIncomingEdges();

        IEnumerable<IEdge> INode.GetOutgoingEdges() => GetOutgoingEdges();

        IEnumerable<INode> INode.GetPredecessors() => GetPredecessors();

        IEnumerable<INode> INode.GetSuccessors() => GetSuccessors();

    }
}