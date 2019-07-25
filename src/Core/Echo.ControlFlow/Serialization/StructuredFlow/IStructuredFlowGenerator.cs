using Echo.ControlFlow.Specialized;
using Echo.ControlFlow.Specialized.Blocks;

namespace Echo.ControlFlow.Serialization.StructuredFlow
{
    /// <summary>
    /// Provides members for transforming control flow graphs into structured blocks of code.  
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that the control flow graph stores.</typeparam>
    public interface IStructuredFlowGenerator<TInstruction>
    {
        /// <summary>
        /// Converts a control flow graph into structured blocks of code. 
        /// </summary>
        /// <param name="cfg">The control flow graph to convert.</param>
        /// <returns>The structured blocks of code.</returns>
        ScopeBlock<TInstruction> Generate(ControlFlowGraph<TInstruction> cfg);
    }
}