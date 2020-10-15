using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.Core.Code;

namespace Echo.ControlFlow.Editing.Synchronization
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
        /// <param name="flags">The flags that dictate the strategy used for pulling basic block updates into a control flow graph.</param>
        public FlowControlSynchronizer(
            ControlFlowGraph<TInstruction> cfg,
            IStaticSuccessorResolver<TInstruction> successorResolver, 
            FlowControlSynchronizationFlags flags)
        {
            ControlFlowGraph = cfg ?? throw new ArgumentNullException(nameof(cfg));
            SuccessorResolver = successorResolver ?? throw new ArgumentNullException(nameof(successorResolver));
            Flags = flags;
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
        /// Gets the flags that dictate the strategy used for pulling basic block updates into a control flow graph.
        /// </summary>
        public FlowControlSynchronizationFlags Flags
        {
            get;
        }

        /// <summary>
        /// Traverses all nodes in the control flow graph, and synchronizes the structure of the graph with the contents
        /// of each basic block within the traversed nodes.
        /// </summary>
        public bool UpdateFlowControl()
        {
            var transaction = new ControlFlowGraphEditTransaction<TInstruction>();
            foreach (var node in ControlFlowGraph.Nodes)
                CheckForChangesInNode(transaction, node);
            
            if (transaction.Count > 0)
            {
                transaction.Apply(ControlFlowGraph);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Pulls any updates from the provided node into the structure of the control flow graph.
        /// </summary>
        /// <param name="node">The node.</param>
        public bool UpdateFlowControl(ControlFlowNode<TInstruction> node)
        {
            var transaction = new ControlFlowGraphEditTransaction<TInstruction>();
            CheckForChangesInNode(transaction, node);
            
            if (transaction.Count > 0)
            {
                transaction.Apply(ControlFlowGraph);
                return true;
            }

            return false;
        }

        private bool CheckForChangesInNode(
            ControlFlowGraphEditTransaction<TInstruction> transaction,
            ControlFlowNode<TInstruction> node)
        {
            bool hasChanges = false;

            if ((Flags & FlowControlSynchronizationFlags.TraverseEntireBasicBlock) != 0)
                hasChanges |= CheckForChangesInBasicBlock(transaction, node);

            hasChanges |= CheckForChangesInFooter(transaction, node);
            return hasChanges;
        }

        private bool CheckForChangesInBasicBlock(ControlFlowGraphEditTransaction<TInstruction> transaction, ControlFlowNode<TInstruction> node)
        {
            bool hasChanges = false;
            
            // Most instructions will have <= 2 successors.
            // - Immediate fallthrough successor or unconditional branch target.
            // - A single conditional branch target.
            
            // The only exception will be switch-like instructions.
            // Therefore we start off by renting a buffer of at least two elements.
            var arrayPool = ArrayPool<SuccessorInfo>.Shared;
            var successorsBuffer = arrayPool.Rent(2);

            try
            {
                var architecture = ControlFlowGraph.Architecture;

                // Skip last instruction (footer), since it is processed somewhere else.
                for (int i = 0; i < node.Contents.Instructions.Count - 1; i++)
                {
                    // Optimization: we assume the architecture has a faster implementation for categorizing instructions
                    // as instructions that influence the basic block (i.e. branches) than the successor resolver itself.
                    var instruction = node.Contents.Instructions[i];
                    var flowControl = architecture.GetFlowControl(instruction);
                    if (SplitsBasicBlock(flowControl))
                    {
                        long offset = architecture.GetOffset(instruction);

                        // Split the node after the instruction.
                        var splitAction = new SplitNodeAction<TInstruction>(offset + architecture.GetSize(instruction));
                        transaction.EnqueueAction(splitAction);

                        // Remove the fallthrough edge generated by ControlFlowNode<T>.SplitAtIndex.
                        transaction.EnqueueAction(new UpdateFallThroughAction<TInstruction>(offset, null));
                        
                        // Verify that our buffer has enough elements.
                        int successorCount = SuccessorResolver.GetSuccessorsCount(instruction);
                        if (successorsBuffer.Length < successorCount)
                        {
                            arrayPool.Return(successorsBuffer);
                            successorsBuffer = arrayPool.Rent(successorCount);
                        }
                        
                        // Get new successors.
                        var successorBufferSlice = new Span<SuccessorInfo>(successorsBuffer, 0, successorCount);
                        int actualSuccessorCount = SuccessorResolver.GetSuccessors(instruction, successorBufferSlice);
                        if (actualSuccessorCount > successorCount)
                            throw new InvalidOperationException();
                        
                        for (int j = 0; j < actualSuccessorCount; j++)
                        {
                            var successor = successorsBuffer[j];
                            
                            IControlFlowGraphEditAction<TInstruction> action;
                            if (successor.EdgeType == ControlFlowEdgeType.FallThrough)
                            {
                                action = new UpdateFallThroughAction<TInstruction>(
                                    offset,
                                    successor.DestinationAddress);
                            }
                            else
                            {
                                action = new AddEdgeAction<TInstruction>(
                                    offset,
                                    successor.DestinationAddress,
                                    successor.EdgeType);
                            }

                            transaction.EnqueueAction(action);
                        }

                        hasChanges = true;
                    }
                }
            }
            finally
            {
                arrayPool.Return(successorsBuffer);
            }

            return hasChanges;
        }

        private bool CheckForChangesInFooter(
            ControlFlowGraphEditTransaction<TInstruction> transaction,
            ControlFlowNode<TInstruction> node)
        {
            bool hasChanges = false;
            
            // Most instructions will have <= 2 successors.
            // - Immediate fallthrough successor or unconditional branch target.
            // - A single conditional branch target.
            
            // The only exception will be switch-like instructions.
            // Therefore we start off by renting a buffer of at least two elements.
            var arrayPool = ArrayPool<SuccessorInfo>.Shared;
            
            // Verify that our buffer has enough elements.
            int successorCount = SuccessorResolver.GetSuccessorsCount(node.Contents.Footer);
            var successorsBuffer = arrayPool.Rent(successorCount);

            try
            {
                // Get new successors.
                var successorBufferSlice = new Span<SuccessorInfo>(successorsBuffer, 0, successorCount);
                int actualSuccessorCount = SuccessorResolver.GetSuccessors(node.Contents.Footer, successorBufferSlice);
                if (actualSuccessorCount > successorCount)
                    throw new InvalidOperationException();

                // Group successors by type:
                long? fallthroughSuccessor = null;
                var conditionalSuccessors = new List<long>();
                var abnormalSuccessors = new List<long>();

                for (int i = 0; i < actualSuccessorCount; i++)
                {
                    var successor = successorsBuffer[i];
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
                hasChanges |= CheckIfFallThroughChanged(transaction, node, fallthroughSuccessor);
                hasChanges |= CheckIfAdjacencyListChanged(transaction, node.ConditionalEdges, conditionalSuccessors);
                hasChanges |= CheckIfAdjacencyListChanged(transaction, node.AbnormalEdges, abnormalSuccessors);
            }
            finally
            {
                arrayPool.Return(successorsBuffer);
            }

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
                if (node.UnconditionalNeighbour is {})
                    fallThroughChanged = true; // Fallthrough was removed.
            }
            else if (node.UnconditionalNeighbour is null || node.UnconditionalNeighbour.Offset != successorOffset.Value)
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