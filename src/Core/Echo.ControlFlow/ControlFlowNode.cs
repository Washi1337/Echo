using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Regions;
using Echo.Graphing;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Represents a node in a control flow graph, containing a basic block of instructions that are to be executed
    /// in a sequence.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the node.</typeparam>
    public class ControlFlowNode<TInstruction> : IIdentifiedNode
        where TInstruction : notnull
    {
        private ControlFlowEdge<TInstruction>? _unconditionalEdge;

        /// <summary>
        /// Creates a new empty control flow node.
        /// </summary>
        public ControlFlowNode()
            : this(new BasicBlock<TInstruction>())
        {
        }

        /// <summary>
        /// Creates a new empty control flow node for the provided offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        public ControlFlowNode(long offset)
            : this(new BasicBlock<TInstruction>(offset))
        {
        }
        
        /// <summary>
        /// Creates a new control flow node containing the provided basic block of instructions, to be added to the graph.
        /// </summary>
        /// <param name="instructions">The basic block to store in the node.</param>
        public ControlFlowNode(params TInstruction[] instructions)
            : this(new BasicBlock<TInstruction>(instructions))
        {
        }

        /// <summary>
        /// Creates a new control flow node containing the provided basic block of instructions, to be added to the graph.
        /// </summary>
        /// <param name="instructions">The basic block to store in the node.</param>
        public ControlFlowNode(IEnumerable<TInstruction> instructions)
            : this(new BasicBlock<TInstruction>(instructions))
        {
        }

        /// <summary>
        /// Creates a new control flow node containing the provided basic block of instructions, to be added to the graph.
        /// </summary>
        /// <param name="basicBlock">The basic block to store in the node.</param>
        public ControlFlowNode(BasicBlock<TInstruction> basicBlock)
        {
            Contents = basicBlock ?? throw new ArgumentNullException(nameof(basicBlock));
            ConditionalEdges = new AdjacencyCollection<TInstruction>(this, ControlFlowEdgeType.Conditional);
            AbnormalEdges = new AdjacencyCollection<TInstruction>(this, ControlFlowEdgeType.Abnormal);
        }

        /// <summary>
        /// Gets the graph that contains this node, or <c>null</c> if the node is not added to any graph yet.  
        /// </summary>
        public ControlFlowGraph<TInstruction>? ParentGraph
        {
            get
            {
                var region = (IControlFlowRegion<TInstruction>?) ParentRegion;
                while (true)
                {
                    switch (region)
                    {
                        case null:
                            return null;
                        case ControlFlowGraph<TInstruction> graph:
                            return graph;
                        default:
                            region = region.ParentRegion;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the graph region that contains this node, or <c>null</c> if the node is not added to any graph yet.  
        /// </summary>
        public IScopeControlFlowRegion<TInstruction>? ParentRegion
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the offset of the basic block the node is representing.
        /// </summary>
        public long Offset => Contents.Offset;

        long IIdentifiedNode.Id => Offset;

        /// <inheritdoc />
        public int InDegree => IncomingEdges.Count;

        /// <inheritdoc />
        public int OutDegree
        {
            get
            {
                int count = ConditionalEdges.Count + AbnormalEdges.Count;
                if (UnconditionalEdge is {})
                    count++;
                return count;
            }
        }

        /// <summary>
        /// Gets the user-defined contents of this node.
        /// </summary>
        public BasicBlock<TInstruction> Contents
        {
            get;
        }

        /// <summary>
        /// Gets or sets user data that is added to the node.
        /// </summary>
        public object? UserData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the neighbour to which the control is transferred to after execution of this block and no
        /// other condition is met.
        /// </summary>
        public ControlFlowNode<TInstruction>? UnconditionalNeighbour
        {
            get => UnconditionalEdge?.Target;
            set => UnconditionalEdge = value is null ? null : new ControlFlowEdge<TInstruction>(this, value);
        }

        /// <summary>
        /// Gets or sets the edge to the neighbour to which the control is transferred to after execution of this block
        /// and no other condition is met.
        /// </summary>
        public ControlFlowEdge<TInstruction>? UnconditionalEdge
        {
            get => _unconditionalEdge;
            set
            {
                if (value is not null)
                {
                    if (value.Type != ControlFlowEdgeType.FallThrough && value.Type != ControlFlowEdgeType.Unconditional)
                        throw new ArgumentException("New edge must be either a fallthrough edge or an unconditional edge.");
                    AdjacencyCollection<TInstruction>.AssertEdgeValidity(this, value, value.Type);
                }

                _unconditionalEdge?.Target.IncomingEdges.Remove(_unconditionalEdge);
                _unconditionalEdge = value;
                _unconditionalEdge?.Target.IncomingEdges.Add(_unconditionalEdge);
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
        internal IList<ControlFlowEdge<TInstruction>> IncomingEdges
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
            if (neighbour is null)
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
                case ControlFlowEdgeType.Unconditional:
                    if (UnconditionalEdge != null)
                        throw new InvalidOperationException("Node already has an unconditional edge to another node.");
                    UnconditionalEdge = edge;
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
        /// Synchronizes the basic block's offset with the offset of the first instruction.
        /// </summary>
        public void UpdateOffset()
        {
            if (ParentGraph is not null)
                Contents.UpdateOffset(ParentGraph.Architecture);
            else
                Contents.Offset = 0;
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
            if (ParentGraph is null)
                throw new InvalidOperationException("Cannot split a node that is not added to a graph yet.");
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
            var newNode = new ControlFlowNode<TInstruction>(newBlock);
            ParentGraph.Nodes.Add(newNode);
            if (ParentRegion is ScopeRegion<TInstruction> scope)
                newNode.MoveToRegion(scope);
            newNode.ParentRegion = ParentRegion;

            // Remove outgoing edges.
            var edges = GetOutgoingEdges().ToArray();
            ConditionalEdges.Clear();
            AbnormalEdges.Clear();
            UnconditionalNeighbour = newNode;

            // Move removed outgoing edges to new node.
            foreach (var edge in edges)
                newNode.ConnectWith(edge.Target, edge.Type);

            return (this, newNode);
        }

        /// <summary>
        /// Merges the current node with its fallthrough predecessor, by combining the two basic blocks together,
        /// connecting the predecessor with the successors of the current node. and finally removing the current node.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the node could not be merged because it has no fallthrough predecessor, has multiple predecessors,
        /// or the predecessor has multiple successors which prohibit merging.
        /// </exception>
        public void MergeWithPredecessor()
        {
            if (IncomingEdges.Count == 0) 
                throw new InvalidOperationException("Node does not have any predecessors.");
            if (IncomingEdges.Count > 1) 
                throw new InvalidOperationException("Node has too many predecessors.");

            var incomingFallthrough = IncomingEdges
                .FirstOrDefault(e => e.Type == ControlFlowEdgeType.FallThrough);

            if (incomingFallthrough is null)
                throw new InvalidOperationException("The incoming edge of the node is not a fallthrough edge.");

            incomingFallthrough.Origin.MergeWithSuccessor();
        }

        /// <summary>
        /// Merges the current node with its fallthrough neighbour, by combining the two basic blocks together,
        /// connecting the node with the successors of the neighbour. and finally removing the neighbour node.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Occurs when the node could not be merged because it has no fallthrough neighbour, or has multiple successors.
        /// </exception>
        public void MergeWithSuccessor()
        {
            if (ParentGraph is null)
                throw new InvalidOperationException("Node is not added to a graph.");
            
            var successor = UnconditionalNeighbour;
            if (successor is null)
                throw new InvalidOperationException("Node has no fallthrough neighbour to merge into.");
            
            if (ConditionalEdges.Count > 0 || AbnormalEdges.Count > 0)
                throw new InvalidOperationException("Node has conditional and/or abnormal edges.");

            foreach (var instruction in successor.Contents.Instructions)
                Contents.Instructions.Add(instruction);
            successor.Contents.Instructions.Clear();

            UnconditionalNeighbour = null;
            foreach (var edge in successor.GetOutgoingEdges())
                ConnectWith(edge.Target, edge.Type);

            ParentGraph.Nodes.Remove(successor);
        }
        
        /// <summary>
        /// Gets a collection of all edges that target this node.
        /// </summary>
        /// <returns>The incoming edges.</returns>
        public IEnumerable<ControlFlowEdge<TInstruction>> GetIncomingEdges() => IncomingEdges;

        /// <summary>
        /// Gets a collection of all outgoing edges originating from this node.
        /// </summary>
        /// <returns>The outgoing edges.</returns>
        public IEnumerable<ControlFlowEdge<TInstruction>> GetOutgoingEdges()
        {
            var result = new List<ControlFlowEdge<TInstruction>>();
            if (UnconditionalEdge != null)
                result.Add(UnconditionalEdge);
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
            return UnconditionalNeighbour == neighbour
                   || ConditionalEdges.Contains(neighbour)
                   || AbnormalEdges.Contains(neighbour);
        }

        /// <summary>
        /// Removes all incident edges (both incoming and outgoing edges) from the node, effectively isolating the node
        /// in the graph. 
        /// </summary>
        public void Disconnect()
        {
            UnconditionalEdge = null;
            ConditionalEdges.Clear();
            AbnormalEdges.Clear();

            foreach (var incomingEdge in IncomingEdges.ToArray())
            {
                switch (incomingEdge.Type)
                {
                    case ControlFlowEdgeType.FallThrough:
                    case ControlFlowEdgeType.Unconditional:
                        incomingEdge.Origin.UnconditionalEdge = null;
                        break;
                    case ControlFlowEdgeType.Conditional:
                        incomingEdge.Origin.ConditionalEdges.Remove(incomingEdge);
                        break;
                    case ControlFlowEdgeType.Abnormal:
                        incomingEdge.Origin.AbnormalEdges.Remove(incomingEdge);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Removes the node out of any sub region in the graph.
        /// </summary>
        public void RemoveFromAnyRegion()
        {
            if (ParentRegion is not null && ParentRegion != ParentGraph)
                ParentRegion.RemoveNode(this);
        }

        /// <summary>
        /// Moves the node from its current region (if any) into the provided sub region.
        /// </summary>
        /// <param name="region">The region to move the node to.</param>
        public void MoveToRegion(ScopeRegion<TInstruction> region)
        {
            RemoveFromAnyRegion();
            region.Nodes.Add(this);
        }

        /// <summary>
        /// Obtains the parent exception handler region that this node resides in (if any).
        /// </summary>
        /// <returns>
        /// The parent exception handler region, or <c>null</c> if the node is not part of any exception handler.
        /// </returns>
        public ExceptionHandlerRegion<TInstruction>? GetParentExceptionHandler() => ParentRegion?.GetParentExceptionHandler();

        /// <summary>
        /// Obtains the parent handler region that this node resides in (if any).
        /// </summary>
        /// <returns>
        /// The parent handler region, or <c>null</c> if the node is not part of any handler.
        /// </returns>
        public HandlerRegion<TInstruction>? GetParentHandler() => ParentRegion?.GetParentHandler();

        /// <summary>
        /// Traverses the region tree upwards and collects all regions this node is situated in. 
        /// </summary>
        /// <returns>The regions this node is situated in, starting with the inner-most regions.</returns>
        public IEnumerable<IControlFlowRegion<TInstruction>> GetSituatedRegions()
        {
            var parentRegion = (IControlFlowRegion<TInstruction>?) ParentRegion;
            while (parentRegion is not null)
            {
                yield return parentRegion;
                parentRegion = parentRegion.ParentRegion;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the node is in the provided region.
        /// </summary>
        /// <param name="region">The region.</param>
        /// <returns><c>true</c> if the node is within the region, <c>false</c> otherwise.</returns>
        public bool IsInRegion(IControlFlowRegion<TInstruction> region) => GetSituatedRegions().Contains(region);

        /// <inheritdoc />
        public override string ToString() => Contents.Offset.ToString("X8");

        IEnumerable<IEdge> INode.GetIncomingEdges() => GetIncomingEdges();

        IEnumerable<IEdge> INode.GetOutgoingEdges() => GetOutgoingEdges();

        IEnumerable<INode> INode.GetPredecessors() => GetPredecessors();

        IEnumerable<INode> INode.GetSuccessors() => GetSuccessors();

        bool INode.HasPredecessor(INode node) => node is ControlFlowNode<TInstruction> n && HasPredecessor(n);
        
        bool INode.HasSuccessor(INode node) => node is ControlFlowNode<TInstruction> n && HasSuccessor(n);
    }
}