using Echo.ControlFlow.Construction.Static;

namespace Echo.ControlFlow.Editing.Synchronization
{
    /// <summary>
    /// Provides extensions for pulling updates from basic blocks into a control flow graph. This includes splitting
    /// and merging nodes where necessary, as well as adding or removing any edges. 
    /// </summary>
    public static class FlowControlSynchronization
    {
        /// <summary>
        /// Pulls any updates from the basic block embedded in the node, and updates the parent control flow graph
        /// accordingly. 
        /// </summary>
        /// <param name="node">The node to pull updates from.</param>
        /// <param name="successorResolver">The object to use for resolving successors of a single instruction.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
        /// <returns><c>true</c> if any changes were made, <c>false</c> otherwise.</returns>
        public static bool UpdateFlowControl<TInstruction>(
            this ControlFlowNode<TInstruction> node,
            IStaticSuccessorResolver<TInstruction> successorResolver)
        {
            return UpdateFlowControl(node, successorResolver, FlowControlSynchronizationFlags.TraverseEntireBasicBlock);
        }

        /// <summary>
        /// Pulls any updates from the basic block embedded in the node, and updates the parent control flow graph
        /// accordingly. 
        /// </summary>
        /// <param name="node">The node to pull updates from.</param>
        /// <param name="successorResolver">The object to use for resolving successors of a single instruction.</param>
        /// <param name="flags">Flags indicating several options for updating the control flow graph.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
        /// <returns><c>true</c> if any changes were made, <c>false</c> otherwise.</returns>
        public static bool UpdateFlowControl<TInstruction>(
            this ControlFlowNode<TInstruction> node,
            IStaticSuccessorResolver<TInstruction> successorResolver, 
            FlowControlSynchronizationFlags flags)
        {
            var synchronizer = new FlowControlSynchronizer<TInstruction>(node.ParentGraph, successorResolver, flags);
            return synchronizer.UpdateFlowControl(node);
        }

        /// <summary>
        /// Traverses all nodes in the control flow graph, and synchronizes the structure of the graph with the contents
        /// of each basic block within the traversed nodes.
        /// </summary>
        /// <param name="graph">The graph to synchronize.</param>
        /// <param name="successorResolver">The object to use for resolving successors of a single instruction.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
        /// <returns><c>true</c> if any changes were made, <c>false</c> otherwise.</returns>
        public static bool UpdateFlowControl<TInstruction>(
            this ControlFlowGraph<TInstruction> graph,
            IStaticSuccessorResolver<TInstruction> successorResolver)
        {
            return UpdateFlowControl(graph, successorResolver, FlowControlSynchronizationFlags.TraverseEntireBasicBlock);
        }

        /// <summary>
        /// Traverses all nodes in the control flow graph, and synchronizes the structure of the graph with the contents
        /// of each basic block within the traversed nodes.
        /// </summary>
        /// <param name="graph">The graph to synchronize.</param>
        /// <param name="successorResolver">The object to use for resolving successors of a single instruction.</param>
        /// <param name="flags">Flags indicating several options for updating the control flow graph.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
        /// <returns><c>true</c> if any changes were made, <c>false</c> otherwise.</returns>
        public static bool UpdateFlowControl<TInstruction>(
            this ControlFlowGraph<TInstruction> graph,
            IStaticSuccessorResolver<TInstruction> successorResolver,
            FlowControlSynchronizationFlags flags)
        {
            var synchronizer = new FlowControlSynchronizer<TInstruction>(graph, successorResolver, flags);
            return synchronizer.UpdateFlowControl();
        }
    }
}