using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a base for branching instruction handlers.
    /// </summary>
    public abstract class BranchHandlerBase : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
        {
            if (EvaluateCondition(context, instruction))
                context.CurrentFrame.ProgramCounter = ((ICilLabel) instruction.Operand!).Offset;
            else
                context.CurrentFrame.ProgramCounter += instruction.Size;

            return CilDispatchResult.Success();
        }

        /// <summary>
        /// Evaluates the condition for the branching instruction.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="instruction">The instruction to dispatch and evaluate.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> otherwise.</returns>
        protected abstract bool EvaluateCondition(CilExecutionContext context, CilInstruction instruction);
    }
}