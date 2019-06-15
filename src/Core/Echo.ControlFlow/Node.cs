using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.Core.Code;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides a base implementation for a basic block present in a control flow graph, containing a list of
    /// instructions that are executed in sequential order.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store.</typeparam>
    public class Node<TInstruction> : INode
        where TInstruction : IInstruction
    {
        private Edge<TInstruction> _fallThroughEdge;
        IEdge INode.GetFallThroughEdge() => FallThroughEdge;

        /// <summary>
        /// Creates a new empty basic block, to be added to a control flow graph.
        /// </summary>
        public Node()
        : this(Enumerable.Empty<TInstruction>())
        {
        }

        /// <summary>
        /// Creates a new basic block filled with the provided list of instructions, to be added to a control flow graph.
        /// </summary>
        /// <param name="instructions">The instructions to store in the basic block.</param>
        public Node(IEnumerable<TInstruction> instructions)
        {
            Instructions = new List<TInstruction>(instructions);
            ConditionalEdges = new AdjacencyCollection<TInstruction>(this, EdgeType.Conditional);
            AbnormalEdges = new AdjacencyCollection<TInstruction>(this, EdgeType.Abnormal);
        }

        /// <summary>
        /// Gets the graph that contains this node, or <c>null</c> if the node is not added to any graph yet.  
        /// </summary>
        public Graph<TInstruction> ParentGraph
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a collection of instructions that are executed in sequential order when the program reaches this
        /// basic block.
        /// </summary>
        public IList<TInstruction> Instructions
        {
            get;
        }

        /// <summary>
        /// Gets or sets the neighbour to which the control is transferred to after execution of this block and no
        /// other condition is met.
        /// </summary>
        public Node<TInstruction> FallThroughNeighbour
        {
            get => FallThroughEdge?.Target;
            set => FallThroughEdge = new Edge<TInstruction>(this, value);
        }
        
        /// <summary>
        /// Gets or sets the edge to the neighbour to which the control is transferred to after execution of this block
        /// and no other condition is met.
        /// </summary>
        public Edge<TInstruction> FallThroughEdge
        {
            get => _fallThroughEdge;
            set
            {
                if (value != null)
                    AdjacencyCollection<TInstruction>.AssertEdgeValidity(this, EdgeType.FallThrough, value);
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
        public AdjacencyCollection<TInstruction> ConditionalEdges
        {
            get;
        }

        /// <summary>
        /// Gets a collection of abnormal edges that originate from this source.
        /// </summary>
        /// <remarks>
        /// These edges are typically present when a node is part of a region of code protected by an exception handler.
        /// </remarks>
        public AdjacencyCollection<TInstruction> AbnormalEdges
        {
            get;
        }

        /// <summary>
        /// Connects the node to the provided neighbour using a fallthrough edge. 
        /// </summary>
        /// <param name="neighbour">The node to connect to.</param>
        /// <returns>The edge that was used to connect the two nodes together.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="neighbour"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Occurs when the node already contains a fallthrough edge to another node.</exception>
        public Edge<TInstruction> ConnectWith(Node<TInstruction> neighbour)
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
        public Edge<TInstruction> ConnectWith(Node<TInstruction> neighbour, EdgeType edgeType)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            var edge = new Edge<TInstruction>(this, neighbour, edgeType);
            
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
        public IEnumerable<Edge<TInstruction>> GetIncomingEdges()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Gets a collection of all outgoing edges originating from this node.
        /// </summary>
        /// <returns>The outgoing edges.</returns>
        public IEnumerable<Edge<TInstruction>> GetOutgoingEdges()
        {
            var result = new List<Edge<TInstruction>>();
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
        public IEnumerable<Node<TInstruction>> GetPredecessors()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a collection of nodes that might be executed after this node. This includes any node that this node
        /// might transfer control to, regardless of edge type.
        /// </summary>
        /// <returns>The successor nodes.</returns>
        public IEnumerable<Node<TInstruction>> GetSuccessors()
        {
            return GetOutgoingEdges()
                .Select(n => n.Target)
                .Distinct();
        }

        /// <summary>
        /// Determines whether another node is a neighbour of this node.
        /// </summary>
        /// <param name="neighbour">The potential neighbour.</param>
        /// <returns><c>True</c> if the provided node is a neighbour, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Occurs when the provided neighbour is <c>null</c></exception>
        public bool HasNeighbour(Node<TInstruction> neighbour)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            return FallThroughNeighbour == neighbour
                   || ConditionalEdges.Contains(neighbour)
                   || AbnormalEdges.Contains(neighbour);
        }

        IEnumerable<IEdge> INode.GetConditionalEdges() => ConditionalEdges;

        IEnumerable<IEdge> INode.GetAbnormalEdges() => AbnormalEdges;

        IEnumerable<IEdge> INode.GetIncomingEdges() => GetIncomingEdges();

        IEnumerable<IEdge> INode.GetOutgoingEdges() => GetOutgoingEdges();

        IEnumerable<INode> INode.GetPredecessors() => GetPredecessors();

        IEnumerable<INode> INode.GetSuccessors() => GetSuccessors();


    }
}