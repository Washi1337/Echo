using System;
using Echo.DataFlow.Emulation;

namespace Echo.ControlFlow.Construction.Symbolic
{
    /// <summary>
    /// Provides members for resolving the next possible states of a program after the execution of an instruction.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is being executed.</typeparam>
    /// <remarks>
    /// <para>
    /// This interface is meant for components within the Echo project that require information about the transitions
    /// that an individual instruction might apply to a given program state. These are typically control flow graph
    /// builders, such as the <see cref="SymbolicFlowGraphBuilder{TInstruction}"/> class.
    /// </para>
    /// </remarks>
    public interface IStateTransitionResolver<TInstruction>
    {
        /// <summary>
        /// Gets the initial state of the program at a provided entry point address.
        /// </summary>
        /// <param name="entrypointAddress">The entry point address.</param>
        /// <returns>The object representing the initial state of the program.</returns>
        SymbolicProgramState<TInstruction> GetInitialState(long entrypointAddress);

        /// <summary>
        /// Gets the number of transitions the current program state might transition into.
        /// </summary>
        /// <param name="currentState">The current state of the program.</param>
        /// <param name="instruction">The instruction to evaluate.</param>
        /// <param name="context">The context that is used to communicate with the graph builder.</param>
        /// <returns>The number of transitions that the provided instruction might apply.</returns>
        int GetTransitionCount(
            SymbolicProgramState<TInstruction> currentState,
            in TInstruction instruction,
            SymbolicGraphBuilderContext<TInstruction> context);

        /// <summary>
        /// Resolves all possible program state transitions that the provided instruction can apply. 
        /// </summary>
        /// <param name="currentState">The current state of the program.</param>
        /// <param name="instruction">The instruction to evaluate.</param>
        /// <param name="transitionBuffer">The output buffer to write the transitions that the instruction might apply.</param>
        /// <param name="context">The context that is used to communicate with the graph builder.</param>
        /// <returns>The number of transitions that were written into <paramref name="transitionBuffer"/>.</returns>
        int GetTransitions(
            SymbolicProgramState<TInstruction> currentState,
            in TInstruction instruction,
            Span<StateTransition<TInstruction>> transitionBuffer,
            SymbolicGraphBuilderContext<TInstruction> context);
    }
}