using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Represents a single handler region in an exception handler block.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public class HandlerRegion<TInstruction> : ControlFlowRegion<TInstruction>
    {
        private ScopeRegion<TInstruction> _prologue;
        private ScopeRegion<TInstruction> _epilogue;

        /// <summary>
        /// Creates a new instance of the <see cref="HandlerRegion{TInstruction}"/> class without
        /// an explicit prologue and epilogue set.
        /// </summary>
        public HandlerRegion()
        {
            Contents = new ScopeRegion<TInstruction>
            {
                // We need to manually set the parent region here.
                ParentRegion = this
            };
        }
        
        /// <summary>
        /// Gets the region of nodes that form the code that precedes the handler.
        /// </summary>
        /// <remarks>
        /// This region is often used for filter clauses of the exception handler.
        /// </remarks>
        public ScopeRegion<TInstruction> Prologue
        {
            get => _prologue;
            set => UpdateChildRegion(ref _prologue, value);
        }

        /// <summary>
        /// Gets the region of nodes that form the code of the handler block.
        /// </summary>
        public ScopeRegion<TInstruction> Contents
        {
            get;
        }

        /// <summary>
        /// Gets the region of nodes that form the code that proceeds the handler.
        /// </summary>
        public ScopeRegion<TInstruction> Epilogue
        {
            get => _epilogue;
            set => UpdateChildRegion(ref _epilogue, value);
        }

        private void UpdateChildRegion(ref ScopeRegion<TInstruction> field, ScopeRegion<TInstruction> value)
        {
            if (field?.ParentRegion == this)
                field.ParentRegion = null;

            field = value;
            if (value != null)
                field.ParentRegion = this;
        }

        /// <inheritdoc />
        public override ControlFlowNode<TInstruction> GetEntrypoint()
        {
            var entrypoint = _prologue?.GetEntrypoint();
            entrypoint ??= Contents.GetEntrypoint();
            entrypoint ??= _epilogue?.GetEntrypoint();
            return entrypoint;
        }

        /// <inheritdoc />
        public override IEnumerable<ControlFlowRegion<TInstruction>> GetSubRegions()
        {
            if (_prologue != null)
                yield return _prologue;
            yield return Contents;
            if (_epilogue != null)
                yield return _epilogue;
        }

        /// <inheritdoc />
        public override bool RemoveNode(ControlFlowNode<TInstruction> node)
        {
            if (_prologue != null && node.IsInRegion(_prologue))
                return _prologue.RemoveNode(node);
            
            if (_epilogue != null && node.IsInRegion(_epilogue))
                return _epilogue.RemoveNode(node);

            return Contents.RemoveNode(node);
        }

        /// <inheritdoc />
        public override IEnumerable<ControlFlowNode<TInstruction>> GetNodes() =>
            GetSubRegions().SelectMany(r => r.GetNodes());
    }
}