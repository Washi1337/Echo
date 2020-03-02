using System;
using System.Collections.Generic;

namespace Echo.Core.Code
{
    /// <summary>
    /// Provides members for describing an instruction set.
    /// </summary>
    /// <typeparam name="TInstruction">The type of the instruction model this architecture describes.</typeparam>
    public interface IInstructionSetArchitecture<in TInstruction>
    {
        /// <summary>
        /// Gets the offset of an instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the offset from.</param>
        /// <returns>The offset.</returns>
        long GetOffset(TInstruction instruction);

        /// <summary>
        /// Gets the mnemonic of the operation that an instruction performs.
        /// </summary>
        /// <param name="instruction">The instruction to get the mnemonic from.</param>
        /// <returns>The mnemonic.</returns>
        string GetMnemonic(TInstruction instruction);

        /// <summary>
        /// Gets the number of operands of an instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the number of operands from.</param>
        /// <returns>The number of operands.</returns>
        int GetOperandCount(TInstruction instruction);

        /// <summary>
        /// Gets an operand of an instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the operand from.</param>
        /// <param name="index">The operand index.</param>
        /// <returns>The operand.</returns>
        /// <exception cref="IndexOutOfRangeException">Occurs when the provided index was outside the bounds of the range
        /// of operands.</exception>
        object GetOperand(TInstruction instruction, int index);

        /// <summary>
        /// Gets the size in bytes of an instruction.
        /// </summary>
        /// <param name="instruction">The instruction to measure.</param>
        /// <returns>The size.</returns>
        int GetSize(TInstruction instruction);

        /// <summary>
        /// Gets the bytes of the operation code of an instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the operation code bytes from.</param>
        /// <returns>The operation code bytes.</returns>
        byte[] GetOpCodeBytes(TInstruction instruction);
        
        /// <summary>
        /// Gets the bytes of all operands of an instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the operand bytes from.</param>
        /// <returns>The operand bytes.</returns>
        byte[] GetOperandBytes(TInstruction instruction);

        /// <summary>
        /// Gets the bytes that encode the provided instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the bytes from.</param>
        /// <returns>The instruction bytes.</returns>
        byte[] GetInstructionBytes(TInstruction instruction);

        /// <summary>
        /// Gets attributes associated to the instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the attributes from.</param>
        /// <returns>The attributes.</returns>
        InstructionAttributes GetAttributes(TInstruction instruction);
        
        /// <summary>
        /// Gets a value indicating the number of values an instruction pushes on the stack.
        /// </summary>
        /// <param name="instruction">The instruction to get the stack push count from.</param>
        /// <returns>The number of stack slots the instruction pushes.</returns>
        int GetStackPushCount(TInstruction instruction);

        /// <summary>
        /// Gets a value indicating the number of values an instruction pops from the stack.
        /// </summary>
        /// <param name="instruction">The instruction to get the stack pop count from.</param>
        /// <returns>The number of stack slots the instruction pops.</returns>
        int GetStackPopCount(TInstruction instruction);

        /// <summary>
        /// Gets a collection of variables that an instruction reads from.
        /// </summary>
        /// <param name="instruction">The instruction to get the variables from.</param>
        /// <returns>The variables this instruction reads from.</returns>
        IEnumerable<IVariable> GetReadVariables(TInstruction instruction);

        /// <summary>
        /// Gets a collection of variables that an instruction writes to.
        /// </summary>
        /// <param name="instruction">The instruction to get the variables from.</param>
        /// <returns>The variables this instruction writes to.</returns>
        IEnumerable<IVariable> GetWrittenVariables(TInstruction instruction);
    }
}