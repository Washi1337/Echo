using System;

namespace Echo.ControlFlow.Editing
{
    /// <summary>
    /// Represents an action that edits a control flow graph by updating the fallthrough edge of a single node.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    public class UpdateFallThroughAction<TInstruction> : IControlFlowGraphEditAction<TInstruction>
    {
        private bool _isApplied;
        private bool _hasSplit;
        private long? _oldFallThroughOffset;

        /// <summary>
        /// Creates a new instance of the <see cref="UpdateFallThroughAction{TInstruction}" /> class.
        /// </summary>
        /// <param name="branchOffset">The offset of the branching instruction.</param>
        /// <param name="newFallThroughOffset">The offset to the new fallthrough neighbour, or <c>null</c> to remove the fallthrough edge.</param>
        public UpdateFallThroughAction(long branchOffset, long? newFallThroughOffset)
        {
            BranchOffset = branchOffset;
            NewFallThroughOffset = newFallThroughOffset;
        }
        
        /// <summary>
        /// Gets the offset to the branching instruction responsible for the fallthrough edge.
        /// </summary>
        public long BranchOffset
        {
            get;
        }

        /// <summary>
        /// Gets the offset to the new fallthrough neighbour. When this value is <c>null</c>, the removal of the
        /// fallthrough edge is indicated.
        /// </summary>
        public long? NewFallThroughOffset
        {
            get;
        }

        /// <inheritdoc />
        public void Apply(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (_isApplied)
                throw new InvalidOperationException("Operation is already applied.");
            
            var node = context.FindNode(BranchOffset);
            
            // Save original fallthrough node offset.
            _oldFallThroughOffset = node.UnconditionalNeighbour?.Offset; 
                
            // Set new fallthrough neighbour.
            node.UnconditionalNeighbour = NewFallThroughOffset.HasValue
                ? context.FindNodeOrSplit(NewFallThroughOffset.Value, out _hasSplit)
                : null;
            
            _isApplied = true;
        }

        /// <inheritdoc />
        public void Revert(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (!_isApplied)
                throw new InvalidOperationException("Operation is not applied yet.");
            
            var node = context.FindNode(BranchOffset);
            
            // Re-merge node if it was split.
            if (_hasSplit)
            {
                var newNeighbour = node.UnconditionalNeighbour;
                node.UnconditionalNeighbour = null;
                context.RemoveNodeFromIndex(newNeighbour.Offset);
                newNeighbour.MergeWithPredecessor();
            }

            // Restore original fallthrough neighbour.
            node.UnconditionalNeighbour = _oldFallThroughOffset.HasValue 
                ? context.FindNode(_oldFallThroughOffset.Value) 
                : null;
            
            _isApplied = false;
        }

        /// <inheritdoc />
        public override string ToString() => NewFallThroughOffset.HasValue
            ? $"Set fallthrough neighbour of {BranchOffset:X8} to {NewFallThroughOffset:X8}."
            : $"Remove fallthrough neighbour of {BranchOffset:X8}.";
    }
}