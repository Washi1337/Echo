using System.Collections.Generic;
using Echo.Core.Emulation;
using Echo.Core.Values;

namespace Echo.Core.Code
{
    /// <summary>
    /// Represents a single instruction 
    /// </summary>
    public interface IInstruction
    {
        /// <summary>
        /// Gets the offset where the instruction is located in the memory buffer.
        /// </summary>
        long Offset
        {
            get;
        }
        
        /// <summary>
        /// Gets the string representation of the operation code of the instruction.
        /// </summary>
        string Mnemonic
        {
            get;
        }

        /// <summary>
        /// Gets the operands of the instruction.
        /// </summary>
        IList<object> Operand
        {
            get;
        }

        /// <summary>
        /// Gets the number of bytes that the instruction uses to encode itself.
        /// </summary>
        int Size
        {
            get;
        }

        /// <summary>
        /// Gets the bytes that encode the operation code of the instruction.
        /// </summary>
        /// <returns>The bytes.</returns>
        IEnumerable<byte> GetOpCodeBytes();

        /// <summary>
        /// Gets the bytes that encode the operand(s) of the instruction.
        /// </summary>
        /// <returns></returns>
        IEnumerable<byte> GetOperandBytes();

        /// <summary>
        /// Gets the bytes that encode this instruction.
        /// </summary>
        /// <returns></returns>
        IEnumerable<byte> GetBytes();

        /// <summary>
        /// Gets a value indicating the number of values this instruction pushes on the stack.
        /// </summary>
        /// <returns>The number of stack slots the instruction pushes.</returns>
        int GetStackPushCount();

        /// <summary>
        /// Gets a value indicating the number of values this instruction pops from the stack.
        /// </summary>
        /// <returns>The number of stack slots the instruction pops.</returns>
        int GetStackPopCount();

        /// <summary>
        /// Gets a collection of variables that this instruction reads from.
        /// </summary>
        /// <returns>The variables this instruction reads from.</returns>
        IEnumerable<IVariable> GetReadVariables();

        /// <summary>
        /// Gets a collection of variables that this instruction writes to.
        /// </summary>
        /// <returns>The variables this instruction writes to.</returns>
        IEnumerable<IVariable> GetWrittenVariables();
    }
}