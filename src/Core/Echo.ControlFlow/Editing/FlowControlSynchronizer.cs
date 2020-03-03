using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.Core.Code;

namespace Echo.ControlFlow.Editing
{
    /// <summary>
    /// Provides a mechanism for pulling updates from basic blocks into a control flow graph. This includes splitting
    /// and merging nodes where necessary, as well as adding or removing any edges. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the graph contains.</typeparam>
    public class FlowControlSynchronizer<TInstruction>
    {
        private readonly List<long> _nodeOffsets = new List<long>();
        
        /// <summary>
        /// Creates a new instance of the <see cref="FlowControlSynchronizer{TInstruction}"/> class.
        /// </summary>
        /// <param name="cfg">The control flow graph to update.</param>
        /// <param name="successorResolver">The object responsible for resolving successors of a single instruction.</param>
        public FlowControlSynchronizer(ControlFlowGraph<TInstruction> cfg,
            IStaticSuccessorResolver<TInstruction> successorResolver)
        {
            ControlFlowGraph = cfg ?? throw new ArgumentNullException(nameof(cfg));
            SuccessorResolver = successorResolver ?? throw new ArgumentNullException(nameof(successorResolver));
        }
        
        /// <summary>
        /// Gets the control flow graph that needs to be updated.
        /// </summary>
        public ControlFlowGraph<TInstruction> ControlFlowGraph
        {
            get;
        }

        /// <summary>
        /// Gets the object responsible for resolving successors of a single instruction. 
        /// </summary>
        public IStaticSuccessorResolver<TInstruction> SuccessorResolver
        {
            get;
        }

        /// <summary>
        /// Traverses all nodes in the control flow graph, and synchronizes the structure of the graph with the contents
        /// of each basic block within the traversed nodes.
        /// </summary>
        public bool UpdateFlowControl()
        {
            bool changedAtLeastOnce = false;
            
            // Build up the index of nodes so we can find them quickly later.
            BuildNodeOffsetIndex();
            
            // Continue the process until no more changes are applied.
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var node in ControlFlowGraph.Nodes.ToArray())
                    changed |= UpdateFlowControl(node);
                changedAtLeastOnce |= changed;
            }

            return changedAtLeastOnce;
        }

        /// <summary>
        /// Pulls any updates regarding flow control from the basic block of the provided node into the graph.  
        /// </summary>
        /// <param name="node">The node to pull updates from.</param>
        /// <returns><c>true</c> if any new updates were pulled from the node and the graph has changed, <c>false</c> otherwise.</returns>
        public bool UpdateFlowControl(ControlFlowNode<TInstruction> node)
        {
            bool changed = false;
            
            // TODO: check branches inside a basic block.

            changed |=  UpdateFooterFlowControl(node);

            return changed;
        }

        private bool UpdateFooterFlowControl(ControlFlowNode<TInstruction> node)
        {
            bool changed = false;
            var successors = SuccessorResolver.GetSuccessors(node.Contents.Footer);

            // Group successors by type:
            SuccessorInfo? fallthroughSuccessor = null;
            var conditionalSuccessors = new List<SuccessorInfo>();
            var abnormalSuccessors = new List<SuccessorInfo>();

            foreach (var successor in successors)
            {
                switch (successor.EdgeType)
                {
                    case ControlFlowEdgeType.FallThrough:
                        if (fallthroughSuccessor.HasValue)
                            throw new ArgumentException("Instruction has multiple fallthrough successors.");
                        else
                            fallthroughSuccessor = successor;
                        break;
                    
                    case ControlFlowEdgeType.Conditional:
                        conditionalSuccessors.Add(successor);
                        break;
                    
                    case ControlFlowEdgeType.Abnormal:
                        abnormalSuccessors.Add(successor);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Update all outgoing edges of the node.
            changed |= UpdateFallThrough(node, fallthroughSuccessor);
            changed |= UpdateAdjacencyLists(node.ConditionalEdges, conditionalSuccessors);
            changed |= UpdateAdjacencyLists(node.AbnormalEdges, abnormalSuccessors);

            return changed;
        }

        private bool UpdateFallThrough(ControlFlowNode<TInstruction> node, in SuccessorInfo? successor)
        {
            if (!successor.HasValue)
            { 
                if (node.FallThroughNeighbour is null)
                    return false;

                // Fall through neighbour was removed.
                node.FallThroughNeighbour = null;
                return true;
            }
            
            if (node.FallThroughNeighbour is null || node.FallThroughNeighbour.Offset != successor.Value.DestinationAddress)
            {
                // Fallthrough neighbour changed.
                node.FallThroughNeighbour = FindNode(successor.Value.DestinationAddress);
                return true;
            }

            return false;
        }

        private bool UpdateAdjacencyLists(AdjacencyCollection<TInstruction> edges, IReadOnlyList<SuccessorInfo> successors)
        {
            bool changed = successors.Count != edges.Count;

            // Count the number of existing edges per neighbour.
            var oldNeighbours = edges
                .GroupBy(x => x.Target.Offset)
                .ToDictionary(x => x.Key, x => x.Count());
            
            // Count the number of new edges per neighbour.
            var newNeighbours = successors
                .GroupBy(x => x.DestinationAddress)
                .ToDictionary(x => x.Key, x => x.Count());

            // Remove any removed neighbours.
            foreach (var entry in oldNeighbours)
            {
                if (!newNeighbours.TryGetValue(entry.Key, out int newCount))
                {
                    edges.Remove(entry.Key);
                    changed = true;
                }
            }

            // Add any new edges.
            foreach (var entry in newNeighbours)
            {
                oldNeighbours.TryGetValue(entry.Key, out int oldCount);
                for (int i = oldCount; i < entry.Value; i++)
                {
                    edges.Add(FindNode(entry.Key));
                    changed = true;
                }
            }
            
            return changed;
        }

        public void BuildNodeOffsetIndex()
        {
            _nodeOffsets.Clear();
            _nodeOffsets.Capacity = ControlFlowGraph.Nodes.Count;
            foreach (var node in ControlFlowGraph.Nodes)
                _nodeOffsets.Add(node.Offset);
            _nodeOffsets.Sort();
        }

        private ControlFlowNode<TInstruction> FindNode(long offset)
        {
            // Shortcut: check if a node with the provided offset exists in the graph first.
            if (ControlFlowGraph.Nodes.Contains(offset))
                return ControlFlowGraph.Nodes[offset];

            // Find the node that contains the offset.
            return FindNodeSlow(offset);
        }

        private ControlFlowNode<TInstruction> FindNodeSlow(long offset)
        {
            int index = FindClosestNodeIndex(offset);
            if (index != -1)
            {
                var node = ControlFlowGraph.Nodes[_nodeOffsets[index]];
                index = FindInstructionIndex(node.Contents.Instructions, offset);
                if (index != -1)
                {
                    var (a, _) = node.SplitAtIndex(index);
                    return a;
                }
            }
            
            throw new ArgumentException($"Node containing offset {offset:X8} was not found.");
        }

        private int FindClosestNodeIndex(long offset)
        {
            int min = 0;
            int max = _nodeOffsets.Count - 1;
            int mid = 0;
            
            while (min <= max)
            {
                mid = (min + max) / 2;
                if (offset < _nodeOffsets[mid])
                    max = mid - 1;
                else if (offset > _nodeOffsets[mid])
                    min = mid + 1;
                else
                    break;
            }

            if (min > max)
                return max;
            
            while (mid < _nodeOffsets.Count - 1)
            {
                if (_nodeOffsets[mid + 1] > offset)
                    return mid;
                mid++;
            }
            
            return -1;
        }

        private int FindInstructionIndex(IList<TInstruction> instructions, long offset)
        {
            var architecture = ControlFlowGraph.Architecture;
            
            int min = 0;
            int max = instructions.Count - 1;

            while (min <= max)
            {
                int mid = (min + max) / 2;
                long currentOffset = architecture.GetOffset(instructions[mid]);
                if (offset < currentOffset)
                    max = mid - 1;
                else if (offset > currentOffset)
                    min = mid + 1;
                else
                    return mid;
            }

            return -1;
        }

        private static bool SplitsBasicBlock(InstructionFlowControl flowControl)
        {
            return (flowControl & InstructionFlowControl.CanBranch) != 0
                   || (flowControl & InstructionFlowControl.IsTerminator) != 0;
        }
    }
}