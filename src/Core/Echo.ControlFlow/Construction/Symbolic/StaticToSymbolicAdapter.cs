using System.Collections.Generic;
using Echo.Core.Code;
using Echo.Core.Emulation;
using Echo.DataFlow.Values;

namespace Echo.ControlFlow.Construction.Symbolic
{
    /// <summary>
    /// Provides an implementation of an adapter that maps a <see cref="IStaticInstructionProvider{TInstruction}"/>
    /// to a <see cref="ISymbolicInstructionProvider{TInstruction}"/>, by using the program counter stored in the
    /// program state as an offset to look up the current instruction.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that this collection provides.</typeparam>
    public class StaticToSymbolicAdapter<TInstruction> : ISymbolicInstructionProvider<TInstruction>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="StaticToSymbolicAdapter{TInstruction}"/> adapter.
        /// </summary>
        /// <param name="architecture">The architecture of the instructions.</param>
        /// <param name="instructions">The instructions.</param>
        public StaticToSymbolicAdapter(IInstructionSetArchitecture<TInstruction> architecture, IEnumerable<TInstruction> instructions)
            : this(new ListInstructionProvider<TInstruction>(architecture, instructions))
        {
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="StaticToSymbolicAdapter{TInstruction}"/> adapter.
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        public StaticToSymbolicAdapter(IStaticInstructionProvider<TInstruction> instructions)
        {
            Instructions = instructions;
        }

        /// <summary>
        /// Gets the underlying static instructions provider.
        /// </summary>
        public IStaticInstructionProvider<TInstruction> Instructions
        {
            get;
        }

        /// <inheritdoc />
        public IInstructionSetArchitecture<TInstruction> Architecture => Instructions.Architecture;

        /// <inheritdoc />
        public TInstruction GetCurrentInstruction(IProgramState<SymbolicValue<TInstruction>> currentState) =>
            Instructions.GetInstructionAtOffset(currentState.ProgramCounter);
    }
}