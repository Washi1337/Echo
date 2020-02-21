using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Collections;
using Echo.Core.Code;
using Echo.Core.Graphing;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Represents a node in a control flow graph, containing a basic block of instructions that are to be executed
    /// in a sequence.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data to store in the node.</typeparam>
    public class ControlFlowNode<TInstruction> : INode
    {
        private ControlFlowEdge<TInstruction> _fallThroughEdge;


        /// <summary>
        /// Creates a new control flow graph node with an empty basic block, to be added to the graph.
        /// </summary>
        /// <param name="offset">The offset of the node.</param>
        public ControlFlowNode(long offset)
            : this(offset, new BasicBlock<TInstruction>(offset))
        {
        }

        /// <summary>
        /// Creates a new control flow node containing the provided basic block of instructions, to be added to the graph.
        /// </summary>
        /// <param name="offset">The offset of the node.</param>
        /// <param name="instructions">The basic block to store in the node.</param>
        public ControlFlowNode(long offset, params TInstruction[] instructions)
            : this(offset, instructions.AsEnumerable())
        {
        }

        /// <summary>
        /// Creates a new control flow node containing the provided basic block of instructions, to be added to the graph.
        /// </summary>
        /// <param name="offset">The offset of the node.</param>
        /// <param name="instructions">The basic block to store in the node.</param>
        public ControlFlowNode(long offset, IEnumerable<TInstruction> instructions)
            : this(offset, new BasicBlock<TInstruction>(0, instructions))
        {
        }

        /// <summary>
        /// Creates a new control flow node containing the provided basic block of instructions, to be added to the graph.
        /// </summary>
        /// <param name="offset">The offset of the node.</param>
        /// <param name="basicBlock">The basic block to store in the node.</param>
        public ControlFlowNode(long offset, BasicBlock<TInstruction> basicBlock)
        {
            Offset = offset;
            Contents = basicBlock ?? throw new ArgumentNullException(nameof(basicBlock));
            ConditionalEdges = new AdjacencyCollection<TInstruction>(this, ControlFlowEdgeType.Conditional);
            AbnormalEdges = new AdjacencyCollection<TInstruction>(this, ControlFlowEdgeType.Abnormal);
        }

        /// <summary>
        /// Gets the graph that contains this node, or <c>null</c> if the node is not added to any graph yet.  
        /// </summary>
        public ControlFlowGraph<TInstruction> ParentGraph
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the offset of the node.
        /// </summary>
        public long Offset
        {
            get;
        }

        /// <inheritdoc />
        long INode.Id => Offset;

        /// <summary>
        /// Gets the user-defined contents of this node.
        /// </summary>
        public BasicBlock<TInstruction> Contents
        {
            get;
        }

        /// <summary>
        /// Gets or sets the neighbour to which the control is transferred to after execution of this block and no
        /// other condition is met.
        /// </summary>
        public ControlFlowNode<TInstruction> FallThroughNeighbour
        {
            get => FallThroughEdge?.Target;
            set => FallThroughEdge = new ControlFlowEdge<TInstruction>(this, value);
        }

        /// <summary>
        /// Gets or sets the edge to the neighbour to which the control is transferred to after execution of this block
        /// and no other condition is met.
        /// </summary>
        public ControlFlowEdge<TInstruction> FallThroughEdge
        {
            get => _fallThroughEdge;
            set
            {
                if (value is {})
                    AdjacencyCollection<TInstruction>.AssertEdgeValidity(this, ControlFlowEdgeType.FallThrough, value);

                _fallThroughEdge?.Target.IncomingEdges.Remove(_fallThroughEdge);
                _fallThroughEdge = value;
                _fallThroughEdge?.Target.IncomingEdges.Add(value);

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
        /// Provides a record of all incoming edges.
        /// </summary>
        /// <remarks>
        /// This property is automatically updated by the adjacency lists and the fall through edge property associated
        /// to all nodes that might connect themselves to the current node. Do not change it in this class.
        /// </remarks>
        internal ICollection<ControlFlowEdge<TInstruction>> IncomingEdges
        {
            get;
        } = new List<ControlFlowEdge<TInstruction>>();

        /// <summary>
        /// Connects the node to the provided neighbour using a fallthrough edge. 
        /// </summary>
        /// <param name="neighbour">The node to connect to.</param>
        /// <returns>The edge that was used to connect the two nodes together.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="neighbour"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Occurs when the node already contains a fallthrough edge to another node.</exception>
        public ControlFlowEdge<TInstruction> ConnectWith(ControlFlowNode<TInstruction> neighbour)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            return ConnectWith(neighbour, ControlFlowEdgeType.FallThrough);
        }
        
        /// <summary>
        /// Connects the node to the provided neighbour. 
        /// </summary>
        /// <param name="neighbour">The node to connect to.</param>
        /// <param name="edgeType">The type of edge to use for connecting to the other node.</param>
        /// <returns>The edge that was used to connect the two nodes together.</returns>
        /// <exception cref="ArgumentNullException">Occurs when <paramref name="neighbour"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        ///     Occurs when <paramref name="edgeType"/> equals <see cref="ControlFlowEdgeType.FallThrough"/>, and the node
        ///     already contains a fallthrough edge to another node.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Occurs when an invalid edge type was provided.</exception>
        public ControlFlowEdge<TInstruction> ConnectWith(ControlFlowNode<TInstruction> neighbour, ControlFlowEdgeType edgeType)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            var edge = new ControlFlowEdge<TInstruction>(this, neighbour, edgeType);
            
            switch (edgeType)
            {
                case ControlFlowEdgeType.FallThrough:
                    if (FallThroughEdge != null)
                        throw new InvalidOperationException("Node already has a fallthrough edge to another node.");
                    FallThroughEdge = edge;
                    break;

                case ControlFlowEdgeType.Conditional:
                    ConditionalEdges.Add(edge);
                    break;
                
                case ControlFlowEdgeType.Abnormal:
                    AbnormalEdges.Add(edge);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(edgeType), edgeType, null);
            }

            return edge;
        }

        /// <summary>
        /// Splits the node and its embedded basic block in two nodes at the provided index, and connects the two
        /// resulting nodes with a fallthrough edge.
        /// </summary>
        /// <param name="index">The index of the instruction</param>
        /// <returns>The two resulting nodes.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the node cannot be split any further.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Occurs when the provided index falls outside the range of the instructions in the embedded basic block.
        /// </exception>
        public (ControlFlowNode<TInstruction> First, ControlFlowNode<TInstruction> Second) SplitAtIndex(int index)
        {
            if (Contents.Instructions.Count < 2)
                throw new InvalidOperationException("Cannot split up a node with less than two instructions.");
            if (index <= 0 || index >= Contents.Instructions.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            // Collect instructions.
            var instructions = new List<TInstruction>(Contents.Instructions.Count - index);
            while (Contents.Instructions.Count != index)
            {
                instructions.Add(Contents.Instructions[index]);
                Contents.Instructions.RemoveAt(index);
            }
            
            // Create and add new node.
            var newBlock = new BasicBlock<TInstruction>(ParentGraph.Architecture.GetOffset(instructions[0]), instructions);
            var newNode = new ControlFlowNode<TInstruction>(newBlock.Offset, newBlock);
            ParentGraph.Nodes.Add(newNode);

            // Remove outgoing edges.
            var edges = GetOutgoingEdges().ToArray();
            ConditionalEdges.Clear();
            AbnormalEdges.Clear();
            FallThroughNeighbour = newNode;

            // Move removed outgoing edges to new node.
            foreach (var edge in edges)
            {
                switch (edge.Type)
                {
                    case ControlFlowEdgeType.FallThrough:
                        newNode.FallThroughNeighbour = edge.Target;
                        break;
                    case ControlFlowEdgeType.Conditional:
                        newNode.ConditionalEdges.Add(edge.Target);
                        break;
                    case ControlFlowEdgeType.Abnormal:
                        newNode.AbnormalEdges.Add(edge.Target);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return (this, newNode);
        }
        
        /// <summary>
        /// Gets a collection of all edges that target this node.
        /// </summary>
        /// <returns>The incoming edges.</returns>
        public IEnumerable<ControlFlowEdge<TInstruction>> GetIncomingEdges()
        {
            return IncomingEdges;
        }
        
        /// <summary>
        /// Gets a collection of all outgoing edges originating from this node.
        /// </summary>
        /// <returns>The outgoing edges.</returns>
        public IEnumerable<ControlFlowEdge<TInstruction>> GetOutgoingEdges()
        {
            var result = new List<ControlFlowEdge<TInstruction>>();
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
        public IEnumerable<ControlFlowNode<TInstruction>> GetPredecessors()
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
        public IEnumerable<ControlFlowNode<TInstruction>> GetSuccessors()
        {
            return GetOutgoingEdges()
                .Select(n => n.Target)
                .Distinct();
        }
        
        /// <summary>
        /// Determines whether another node is a predecessor of this node.
        /// </summary>
        /// <param name="neighbour">The potential predecessor.</param>
        /// <returns><c>True</c> if the provided node is a predecessor, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Occurs when the provided predecessor is <c>null</c></exception>
        public bool HasPredecessor(ControlFlowNode<TInstruction> neighbour)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            
            // We don't use IncomingEdges here as this requires a linear search.
            return neighbour.HasSuccessor(this);
        }

        /// <summary>
        /// Determines whether another node is a successor of this node.
        /// </summary>
        /// <param name="neighbour">The potential successor.</param>
        /// <returns><c>True</c> if the provided node is a successor, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Occurs when the provided successor is <c>null</c></exception>
        public bool HasSuccessor(ControlFlowNode<TInstruction> neighbour)
        {
            if (neighbour == null)
                throw new ArgumentNullException(nameof(neighbour));
            return FallThroughNeighbour == neighbour
                   || ConditionalEdges.Contains(neighbour)
                   || AbnormalEdges.Contains(neighbour);
        }

        /// <inheritdoc />
        public override string ToString() => Offset.ToString("X8");

        IEnumerable<IEdge> INode.GetIncomingEdges() => GetIncomingEdges();

        IEnumerable<IEdge> INode.GetOutgoingEdges() => GetOutgoingEdges();

        IEnumerable<INode> INode.GetPredecessors() => GetPredecessors();

        IEnumerable<INode> INode.GetSuccessors() => GetSuccessors();

        bool INode.HasPredecessor(INode node) => node is ControlFlowNode<TInstruction> n && HasPredecessor(n);
        
        bool INode.HasSuccessor(INode node) => node is ControlFlowNode<TInstruction> n && HasSuccessor(n);
    }
}