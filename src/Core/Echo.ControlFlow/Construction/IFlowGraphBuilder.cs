using System;
using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// Provides members for building a control flow graph, starting at a specific entrypoint address.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that the control flow graph will contain.</typeparam>
    public interface IFlowGraphBuilder<TInstruction>
    {
        /// <summary>
        /// Constructs a control flow graph, starting at the provided entrypoint address.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <param name="knownBlockHeaders">A list of known block headers that should be included in the traversal.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        ControlFlowGraph<TInstruction> ConstructFlowGraph(
            IInstructionProvider<TInstruction> instructions,
            long entrypoint, 
            IEnumerable<long> knownBlockHeaders);
    }

    /// <summary>
    /// Provides extensions to control flow graph builder implementations.
    /// </summary>
    public static class FlowGraphBuilderExtensions
    {
        /// <summary>
        /// Constructs a control flow graph, starting at the provided entrypoint address.
        /// </summary>
        /// <param name="self">The control flow graph builder to use.</param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        public static ControlFlowGraph<TInstruction> ConstructFlowGraph<TInstruction>(
            this IFlowGraphBuilder<TInstruction> self, 
            IInstructionProvider<TInstruction> instructions, 
            long entrypoint)
        {
            return self.ConstructFlowGraph(instructions, entrypoint, Array.Empty<long>());
        }

        /// <summary>
        /// Constructs a control flow graph, starting at the provided entrypoint address.
        /// </summary>
        /// <param name="self">The control flow graph builder to use.</param>
        /// <param name="instructions">The instructions.</param>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <param name="knownBlockHeaders">A list of known block headers that should be included in the traversal.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        public static ControlFlowGraph<TInstruction> ConstructFlowGraph<TInstruction>(
            this IFlowGraphBuilder<TInstruction> self,
            IInstructionProvider<TInstruction> instructions, 
            long entrypoint, 
            params long[] knownBlockHeaders)
        {
            return self.ConstructFlowGraph(instructions, entrypoint, knownBlockHeaders);
        }

    }
}