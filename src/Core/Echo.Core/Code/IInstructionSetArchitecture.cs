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
        /// Gets the size in bytes of an instruction.
        /// </summary>
        /// <param name="instruction">The instruction to measure.</param>
        /// <returns>The size.</returns>
        int GetSize(TInstruction instruction);
        
        /// <summary>
        /// Gets attributes associated to the flow control behaviour of the provided instruction.
        /// </summary>
        /// <param name="instruction">The instruction to get the attributes from.</param>
        /// <returns>The flow control attributes.</returns>
        InstructionFlowControl GetFlowControl(TInstruction instruction);
        
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