using Echo.Core.Code;
using Echo.Core.Emulation;
using Echo.DataFlow.Emulation;

namespace Echo.ControlFlow.Construction.Symbolic
{
    /// <summary>
    /// Provides members for obtaining instructions based on the current state of a program.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that this collection provides.</typeparam>
    public interface ISymbolicInstructionProvider<TInstruction>
    {
        /// <summary>
        /// Gets the architecture describing the instructions exposed by this instruction provider.
        /// </summary>
        IInstructionSetArchitecture<TInstruction> Architecture
        {
            get;
        }
        
        /// <summary>
        /// Gets the current instruction to be evaluated; that is, the instruction at the current value
        /// of the program counter stored in the provided program state.
        /// </summary>
        /// <param name="currentState">The current state of the program.</param>
        /// <returns>The instruction.</returns>
        TInstruction GetCurrentInstruction(in SymbolicProgramState<TInstruction> currentState);
    }
}