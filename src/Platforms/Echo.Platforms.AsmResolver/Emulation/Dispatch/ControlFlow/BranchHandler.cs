using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a base for all branching operation codes.
    /// </summary>
    public abstract class BranchHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public abstract IReadOnlyCollection<CilCode> SupportedOpCodes
        {
            get;
        }

        /// <summary>
        /// Gets the number of arguments the branch pops from the stack.
        /// </summary>
        protected virtual int ArgumentCount => 1;
            
        /// <inheritdoc />
        public DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var result = VerifyCondition(context, instruction);
            
            int newOffset;
            if (result.IsKnown)
            {
                if (result)
                    newOffset = ((ICilLabel) instruction.Operand).Offset;
                else
                    newOffset = instruction.Offset + instruction.Size;
            }
            else
            {
                // TODO: dispatch event, allowing the user to handle unknown branch conditions.
                throw new DispatchException("Branch condition could not be evaluated.");
            }

            var stack = context.ProgramState.Stack;
            for (int i = 0; i < ArgumentCount; i++)
                stack.Pop();
            
            context.ProgramState.ProgramCounter = newOffset;
            return DispatchResult.Success();
        }

        /// <summary>
        /// Determines whether the branch condition has been met.
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> if not, and <see cref="Trilean.Unknown"/>
        /// if the conclusion is unknown.</returns>
        public abstract Trilean VerifyCondition(CilExecutionContext context, CilInstruction instruction);
    }
}