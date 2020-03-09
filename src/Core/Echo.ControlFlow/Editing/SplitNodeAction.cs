using System;

namespace Echo.ControlFlow.Editing
{
    /// <summary>
    /// Represents an action that edits a control flow graph by splitting a node into two halves.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    public class SplitNodeAction<TInstruction> : IControlFlowGraphEditAction<TInstruction>
    {
        private bool _isApplied;
        private bool _hasSplit;

        /// <summary>
        /// Creates a new instance of the <see cref="SplitNodeAction{TInstruction}"/> class.
        /// </summary>
        /// <param name="splitOffset">The offset to split at.</param>
        public SplitNodeAction(long splitOffset)
        {
            SplitOffset = splitOffset;
        }

        /// <summary>
        /// Gets the offset to split the node at.
        /// </summary>
        public long SplitOffset
        {
            get;
        }

        /// <inheritdoc />
        public void Apply(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (_isApplied)
                throw new InvalidOperationException("Operation is already applied.");
            context.FindNodeOrSplit(SplitOffset, out _hasSplit);
            _isApplied = true;
        }

        /// <inheritdoc />
        public void Revert(ControlFlowGraphEditContext<TInstruction> context)
        {
            if (!_isApplied)
                throw new InvalidOperationException("Operation is not applied yet.");

            if (_hasSplit)
            {
                var node = context.Graph.Nodes[SplitOffset];
                context.RemoveNodeFromIndex(SplitOffset);
                node.MergeWithPredecessor();
            }
        }

        /// <inheritdoc />
        public override string ToString() => 
            $"Split node at {SplitOffset:X8}.";
    }
}