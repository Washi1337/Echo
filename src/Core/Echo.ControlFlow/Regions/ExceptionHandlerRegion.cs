using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Represents a region in a control flow graph that is protected by an exception handler block. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public class ExceptionHandlerRegion<TInstruction> : ControlFlowRegion<TInstruction>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRegion{TInstruction}"/> class.
        /// </summary>
        public ExceptionHandlerRegion()
        {
            ProtectedRegion = new BasicControlFlowRegion<TInstruction>
            {
                ParentRegion = this
            };
            HandlerRegion = new BasicControlFlowRegion<TInstruction>
            {
                ParentRegion = this
            };
        }
        
        /// <summary>
        /// Gets the region of nodes that is protected by the exception handler. 
        /// </summary>
        public ControlFlowRegion<TInstruction> ProtectedRegion
        {
            get;
        }

        /// <summary>
        /// Gets the region of nodes that form the handler block.
        /// </summary>
        public ControlFlowRegion<TInstruction> HandlerRegion
        {
            get;
        }
        
        /// <inheritdoc />
        public override IEnumerable<ControlFlowNode<TInstruction>> GetNodes() => 
            ProtectedRegion.GetNodes().Union(HandlerRegion.GetNodes());

        /// <inheritdoc />
        public override IEnumerable<ControlFlowRegion<TInstruction>> GetSubRegions()
        {
            return new[]
            {
                ProtectedRegion, HandlerRegion
            };
        }

        /// <inheritdoc />
        public override bool RemoveNode(ControlFlowNode<TInstruction> node) => 
            ProtectedRegion.RemoveNode(node) || HandlerRegion.RemoveNode(node);
    }
}