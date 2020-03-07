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
            if (transaction.Actions.Count > 0)
            {
                transaction.Apply();
                return true;
            }

            return false;
        }

        private ControlFlowGraphEditTransaction<TInstruction> CreateEditTransaction()
        {
            var transaction = new ControlFlowGraphEditTransaction<TInstruction>(ControlFlowGraph);

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

            bool hasChanges = false;
            hasChanges |= CheckIfFallThroughChanged(transaction, node, fallthroughSuccessor);
            return hasChanges;
        }

        private bool CheckIfFallThroughChanged(
            ControlFlowGraphEditTransaction<TInstruction> transaction,
            ControlFlowNode<TInstruction> node, 
            in SuccessorInfo? successor)
        {
            bool fallThroughChanged = false;
            
            if (!successor.HasValue)
            {
                if (node.FallThroughNeighbour is {})
                    fallThroughChanged = true;
            }
            else if (node.FallThroughNeighbour is null || node.FallThroughNeighbour.Offset != successor.Value.DestinationAddress)
            {
                fallThroughChanged = true;
            }

            if (fallThroughChanged)
            {
                var architecture = ControlFlowGraph.Architecture;
                long? newFallThrough = successor?.DestinationAddress;
                
                var update = new UpdateFallThroughAction<TInstruction>(
                    architecture.GetOffset(node.Contents.Footer), 
                    newFallThrough);
                
                transaction.EnqueueAction(update);
            }
            
            return fallThroughChanged;
        }

        private static bool SplitsBasicBlock(InstructionFlowControl flowControl)
        {
            return (flowControl & InstructionFlowControl.CanBranch) != 0
                   || (flowControl & InstructionFlowControl.IsTerminator) != 0;
        }
    }
}