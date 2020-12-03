using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Regions.Detection;
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
        /// Gets the architecture of the instructions to graph.
        /// </summary>
        IInstructionSetArchitecture<TInstruction> Architecture
        {
            get;
        }

        /// <summary>
        /// Constructs a control flow graph, starting at the provided entrypoint address.
        /// </summary>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <param name="knownBlockHeaders">A list of known block headers that should be included in the traversal.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        ControlFlowGraph<TInstruction> ConstructFlowGraph(long entrypoint, IEnumerable<long> knownBlockHeaders);
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
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        public static ControlFlowGraph<TInstruction> ConstructFlowGraph<TInstruction>(
            this IFlowGraphBuilder<TInstruction> self, 
            long entrypoint)
        {
            return self.ConstructFlowGraph(entrypoint, Array.Empty<long>());
        }
   
        /// <summary>
        /// Constructs a control flow graph from a collection of instructions, starting at the provided entrypoint address.
        /// </summary>
        /// <param name="self">The control flow graph builder to use.</param>
        /// <param name="entrypoint">The address of the first instruction to traverse.</param>
        /// <param name="exceptionHandlers">The set of exception handler ranges.</param>
        /// <returns>
        /// The constructed control flow graph, with the entrypoint set to the node containing the entrypoint address
        /// provided in <paramref name="entrypoint"/>.
        /// </returns>
        public static ControlFlowGraph<TInstruction> ConstructFlowGraph<TInstruction>(
            this IFlowGraphBuilder<TInstruction> self,
            long entrypoint, 
            IEnumerable<ExceptionHandlerRange> exceptionHandlers)
        {
            var knownBlockHeaders = new HashSet<long>();
            foreach (var range in exceptionHandlers)
            {
                knownBlockHeaders.Add(range.ProtectedRange.Start);
                knownBlockHeaders.Add(range.PrologueRange.Start);
                knownBlockHeaders.Add(range.HandlerRange.Start);
                knownBlockHeaders.Add(range.EpilogueRange.Start);
            }
            
            return self.ConstructFlowGraph(entrypoint, knownBlockHeaders);
        }
        

    }
}