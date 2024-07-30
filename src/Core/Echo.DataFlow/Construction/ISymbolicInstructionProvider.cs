using Echo.Code;
using Echo.DataFlow.Emulation;

namespace Echo.DataFlow.Construction
{
    /// <summary>
    /// Provides members for obtaining instructions based on the current state of a program.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that this collection provides.</typeparam>
    public interface ISymbolicInstructionProvider<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Gets the architecture describing the instructions exposed by this instruction provider.
        /// </summary>
        IArchitecture<TInstruction> Architecture
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