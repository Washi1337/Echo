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
            var transaction = CreateEditTransaction();
            if (transaction.Count > 0)
            {
                transaction.Apply(ControlFlowGraph);
                return true;
            }

            return false;
        }

        private ControlFlowGraphEditTransaction<TInstruction> CreateEditTransaction()
        {
            var transaction = new ControlFlowGraphEditTransaction<TInstruction>();

            foreach (var node in ControlFlowGraph.Nodes)
                CheckForChangesInNode(transaction, node);
                
            return transaction;
        }

        private bool CheckForChangesInNode(
            ControlFlowGraphEditTransaction<TInstruction> transaction,
            ControlFlowNode<TInstruction> node)
        {
            bool hasChanges = false;
            
            // TODO: check branches inside a basic block.

            hasChanges |= CheckForChangesInFooter(transaction, node);
            return hasChanges;
        }

        private bool CheckForChangesInFooter(
            ControlFlowGraphEditTransaction<TInstruction> transaction,
            ControlFlowNode<TInstruction> node)
        {
            var successors = SuccessorResolver.GetSuccessors(node.Contents.Footer);

            // Group successors by type:
            long? fallthroughSuccessor = null;
            var conditionalSuccessors = new List<long>();
            var abnormalSuccessors = new List<long>();

            foreach (var successor in successors)
            {
                switch (successor.EdgeType)
                {
                    case ControlFlowEdgeType.FallThrough:
                        if (fallthroughSuccessor.HasValue)
                            throw new ArgumentException("Instruction has multiple fallthrough successors.");
                        else
                            fallthroughSuccessor = successor.DestinationAddress;
                        break;

                    case ControlFlowEdgeType.Conditional:
                        conditionalSuccessors.Add(successor.DestinationAddress);
                        break;

                    case ControlFlowEdgeType.Abnormal:
                        abnormalSuccessors.Add(successor.DestinationAddress);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Check if there are any changes to the outgoing edges.
            bool hasChanges = false;
            hasChanges |= CheckIfFallThroughChanged(transaction, node, fallthroughSuccessor);
            hasChanges |= CheckIfAdjacencyListChanged(transaction, node.ConditionalEdges, conditionalSuccessors);
            hasChanges |= CheckIfAdjacencyListChanged(transaction, node.AbnormalEdges, abnormalSuccessors);
            return hasChanges;
        }

        private bool CheckIfFallThroughChanged(
            ControlFlowGraphEditTransaction<TInstruction> transaction,
            ControlFlowNode<TInstruction> node, 
            long? successorOffset)
        {
            bool fallThroughChanged = false;

            if (!successorOffset.HasValue)
            {
                if (node.FallThroughNeighbour is {})
                    fallThroughChanged = true; // Fallthrough was removed.
            }
            else if (node.FallThroughNeighbour is null || node.FallThroughNeighbour.Offset != successorOffset.Value)
            {
                // Fallthrough was added or changed.
                fallThroughChanged = true;
            }

            if (fallThroughChanged)
            {
                var architecture = ControlFlowGraph.Architecture;
                
                var update = new UpdateFallThroughAction<TInstruction>(
                    architecture.GetOffset(node.Contents.Footer), 
                    successorOffset);
                
                transaction.EnqueueAction(update);
            }
            
            return fallThroughChanged;
        }

        private bool CheckIfAdjacencyListChanged(
            ControlFlowGraphEditTransaction<TInstruction> transaction,
            AdjacencyCollection<TInstruction> edges,
            IList<long> successorOffsets)
        {
            var architecture = ControlFlowGraph.Architecture;
            long branchOffset = architecture.GetOffset(edges.Owner.Contents.Footer);
            
            bool hasChanges = successorOffsets.Count != edges.Count;

            // Count the number of existing edges per neighbour.
            var oldNeighbours = edges
                .GroupBy(x => x.Target.Offset)
                .ToDictionary(x => x.Key, x => x.Count());

            // Count the number of new edges per neighbour.
            var newNeighbours = successorOffsets
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());

            // Check if any neighbours were completely removed.
            foreach (var entry in oldNeighbours)
            {
                if (!newNeighbours.ContainsKey(entry.Key))
                {
                    int oldCount = entry.Value;
                    for (int i = 0; i < oldCount; i++)
                    {
                        transaction.EnqueueAction(new RemoveEdgeAction<TInstruction>(
                            branchOffset,
                            entry.Key,
                            edges.EdgeType));
                    }

                    hasChanges = true;
                    break;
                }
            }

            // Check if there are any new neighbours or changes in the count of edges to existing neighbours.
            foreach (var entry in newNeighbours)
            {
                long successorOffset = entry.Key;
                int newCount = entry.Value;

                // Get the original number of edges to the neighbour.
                oldNeighbours.TryGetValue(successorOffset, out int oldCount);

                // Add new edges.
                for (int i = oldCount; i < newCount; i++)
                {
                    transaction.EnqueueAction(new AddEdgeAction<TInstruction>(
                        branchOffset,
                        entry.Key,
                        edges.EdgeType));
                }

                // Remove deleted edges.
                for (int i = newCount; i < oldCount; i++)
                {
                    transaction.EnqueueAction(new RemoveEdgeAction<TInstruction>(
                        branchOffset,
                        entry.Key,
                        edges.EdgeType));
                }
                
            }
            
            return hasChanges;
        }

        private static bool SplitsBasicBlock(InstructionFlowControl flowControl)
        {
            return (flowControl & InstructionFlowControl.CanBranch) != 0
                   || (flowControl & InstructionFlowControl.IsTerminator) != 0;
        }
    }
}