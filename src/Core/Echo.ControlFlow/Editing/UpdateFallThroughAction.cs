using System;

namespace Echo.ControlFlow.Editing
{
    public class UpdateFallThroughAction<TInstruction> : IControlFlowGraphEditAction<TInstruction>
    {
        private bool _isApplied = false;
        private bool _hasSplitted;
        private long? _oldFallThroughOffset;

        public UpdateFallThroughAction(long branchOffset, long? newFallThroughOffset)
        {
            BranchOffset = branchOffset;
            NewFallThroughOffset = newFallThroughOffset;
        }
        
        public long BranchOffset
        {
            get;
        }

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
            _oldFallThroughOffset = node.FallThroughNeighbour?.Offset; 
                
            // Set new fallthrough neighbour.
            node.FallThroughNeighbour = NewFallThroughOffset.HasValue
                ? context.FindNodeOrSplit(NewFallThroughOffset.Value, out _hasSplitted)
                : null;
            
            _isApplied = true;
        }

        /// <inheritdoc />
        public void Revert(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (!_isApplied)
                throw new InvalidOperationException("Operation is not applied yet.");
            
            var node = context.FindNode(BranchOffset);
            
            // Re-merge node if it was splitted.
            if (_hasSplitted)
            {
                var newNeighbour = node.FallThroughNeighbour;
                node.FallThroughNeighbour = null;
                newNeighbour.MergeWithPredecessor();
            }

            // Restore original fallthrough neighbour.
            node.FallThroughNeighbour = _oldFallThroughOffset.HasValue 
                ? context.FindNode(_oldFallThroughOffset.Value) 
                : null;
            
            _isApplied = false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Update fallthrough edge {BranchOffset:X8}.";
        }
    }
}