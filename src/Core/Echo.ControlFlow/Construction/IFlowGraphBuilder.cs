using Echo.ControlFlow.Specialized;
using Echo.Core.Code;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Provides members for building a control flow graph, starting at a specific entrypoint address.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that the control flow graph will contain.</typeparam>
    public interface IFlowGraphBuilder<TInstruction>
        where TInstruction : IInstruction
    {
        /// <summary>
        /// Constructs a control flow graph, starting at the provided entrypoint address.
        /// </summary>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        ControlFlowGraph<TInstruction> ConstructFlowGraph(long entrypoint);
    }
}