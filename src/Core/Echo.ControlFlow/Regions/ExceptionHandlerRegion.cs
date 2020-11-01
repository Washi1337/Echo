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
    {
        private BasicControlFlowRegion<TInstruction> _prologue;
        private BasicControlFlowRegion<TInstruction> _epilogue;
        
        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRegion{TInstruction}"/> class.
        /// </summary>
        public ExceptionHandlerRegion()
        {
            ProtectedRegion = new BasicControlFlowRegion<TInstruction>
            {
                ParentRegion = this
            };
            
            HandlerRegions = new RegionCollection<TInstruction>(this);
        }

        /// <summary>
        /// Gets the region of nodes that is protected by the exception handler. 
        /// </summary>
        public BasicControlFlowRegion<TInstruction> ProtectedRegion
        {
            get;
        }

        /// <summary>
        /// Gets the region of nodes that form the code that precedes the handler(s).
        /// </summary>
        public BasicControlFlowRegion<TInstruction> PrologueRegion
        {
            get => _prologue;
            set
            {
                _prologue = value;
                _prologue.ParentRegion = this;
            }
        }

        /// <summary>
        /// Gets the regions that form the handler blocks.
        /// </summary>
        public RegionCollection<TInstruction> HandlerRegions
        {
            get;
        }

        /// <summary>
        /// Gets the region of nodes that form the code that proceeds the handler(s).
        /// </summary>
        public BasicControlFlowRegion<TInstruction> EpilogueRegion
        {
            get => _epilogue;
            set
            {
                _epilogue = value;
                _epilogue.ParentRegion = this;
            }
        }

        /// <inheritdoc />
        public override ControlFlowNode<TInstruction> GetEntrypoint() => ProtectedRegion.Entrypoint;

        /// <inheritdoc />
        public override IEnumerable<ControlFlowNode<TInstruction>> GetNodes() =>
            GetSubRegions().SelectMany(r => r.GetNodes());

        /// <inheritdoc />
        public override IEnumerable<ControlFlowRegion<TInstruction>> GetSubRegions()
        {
            yield return ProtectedRegion;

            if (PrologueRegion is {})
                yield return PrologueRegion;
            
            foreach (var handlerRegion in HandlerRegions)
                yield return handlerRegion;

            if (EpilogueRegion is {}) 
                yield return EpilogueRegion;
        }

        /// <inheritdoc />
        public override bool RemoveNode(ControlFlowNode<TInstruction> node) =>
            ProtectedRegion.RemoveNode(node) || HandlerRegions.Any(r => r.RemoveNode(node));
    }
}