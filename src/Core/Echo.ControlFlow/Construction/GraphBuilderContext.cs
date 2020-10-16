using Echo.Core.Code;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Helper data structure to pass information between the transition resolver and the graph builder.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the control flow graph.</typeparam>
    public class GraphBuilderContext<TInstruction>
    {
        /// <summary>
        /// Creates a new <see cref="GraphBuilderContext{TInstruction}"/>.
        /// </summary>
        /// <param name="architecture">The architecture of the instructions.</param>
        public GraphBuilderContext(IInstructionSetArchitecture<TInstruction> architecture)
        {
            TraversalResult = new InstructionTraversalResult<TInstruction>(architecture);
        }
        
        /// <summary>
        /// Gets the the collected instructions.
        /// </summary>
        public InstructionTraversalResult<TInstruction> TraversalResult
        {
            get;
        }

        /// <summary>
        /// Adds a new known block header to the list of known block headers.
        /// </summary>
        /// <param name="offset">The offset at which the block header lives.</param>
        public void AddHeader(long offset)
        {
            TraversalResult.BlockHeaders.Add(offset);
        }
    }
}