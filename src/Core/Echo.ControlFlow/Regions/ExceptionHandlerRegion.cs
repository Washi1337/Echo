using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Represents a region in a control flow graph that is protected by an exception handler block. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public class ExceptionHandlerRegion<TInstruction> : ControlFlowRegion<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRegion{TInstruction}"/> class.
        /// </summary>
        public ExceptionHandlerRegion()
        {
            ProtectedRegion = new ScopeRegion<TInstruction>
            {
                // We need to manually set the parent region here.
                ParentRegion = this
            };
            
            Handlers = new RegionCollection<TInstruction, HandlerRegion<TInstruction>>(this);
        }

        /// <summary>
        /// Gets the region of nodes that is protected by the exception handler. 
        /// </summary>
        public ScopeRegion<TInstruction> ProtectedRegion
        {
            get;
        }
        
        /// <summary>
        /// Gets the regions that form the handler blocks.
        /// </summary>
        public RegionCollection<TInstruction, HandlerRegion<TInstruction>> Handlers
        {
            get;
        }

        /// <inheritdoc />
        public override ControlFlowNode<TInstruction>? GetEntryPoint() => ProtectedRegion.EntryPoint;

        /// <inheritdoc />
        public override IEnumerable<ControlFlowNode<TInstruction>> GetNodes() =>
            GetSubRegions().SelectMany(r => r.GetNodes());

        /// <inheritdoc />
        public override IEnumerable<ControlFlowRegion<TInstruction>> GetSubRegions()
        {
            yield return ProtectedRegion;
            
            foreach (var handlerRegion in Handlers)
                yield return handlerRegion;
        }

        /// <inheritdoc />
        public override bool RemoveNode(ControlFlowNode<TInstruction> node) =>
            ProtectedRegion.RemoveNode(node) || Handlers.Any(r => r.RemoveNode(node));
    }
}