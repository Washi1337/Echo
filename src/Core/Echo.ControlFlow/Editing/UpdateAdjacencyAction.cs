using System;

namespace Echo.ControlFlow.Editing
{
    /// <summary>
    /// Represents an action that edits a control flow graph by updating one of the adjacency collections of a single
    /// node.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    public abstract class UpdateAdjacencyAction<TInstruction> : IControlFlowGraphEditAction<TInstruction>
    {
        private bool _isApplied;

        /// <summary>
        /// Initializes the <see cref="UpdateAdjacencyAction{TInstruction}"/> base class.
        /// </summary>
        /// <param name="originOffset">The offset of the branch instruction representing the origin of the edge.</param>
        /// <param name="targetOffset">The offset of the neighbour that the edge targets.</param>
        /// <param name="edgeType">The type of edge.</param>
        protected UpdateAdjacencyAction(long originOffset, long targetOffset, ControlFlowEdgeType edgeType)
        {
            OriginOffset = originOffset;
            TargetOffset = targetOffset;
            EdgeType = edgeType;
        }

        /// <summary>
        /// Gets the offset of the branching instruction representing the origin of the edge.
        /// </summary>
        public long OriginOffset
        {
            get;
        }

        /// <summary>
        /// Gets the offset of the neighbour that the edge targets.
        /// </summary>
        public long TargetOffset
        {
            get;
        }

        /// <summary>
        /// Gets the type of edge.
        /// </summary>
        public ControlFlowEdgeType EdgeType
        {
            get;
        }

        /// <inheritdoc />
        public void Apply(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (_isApplied)
                throw new InvalidOperationException("Operation is already applied.");

            OnApply(context);

            _isApplied = true;
        }

        /// <summary>
        /// Applies the update to the adjacency list of the node.
        /// </summary>
        /// <param name="context">The editing context.</param>
        /// <remarks>
        /// This method is guaranteed to be called before <see cref="Revert"/>.
        /// </remarks>
        protected abstract void OnApply(ControlFlowGraphEditContext<TInstruction> context);

        /// <inheritdoc />
        public void Revert(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (!_isApplied)
                throw new InvalidOperationException("Operation is not applied yet.");

            OnRevert(context);

            _isApplied = false;
        }

        /// <summary>
        /// Reverts the update to the adjacency list of the node.
        /// </summary>
        /// <param name="context">The editing context.</param>
        /// <remarks>
        /// This method is guaranteed to be called after <see cref="Apply"/>.
        /// </remarks>
        protected abstract void OnRevert(ControlFlowGraphEditContext<TInstruction> context);
    }
}