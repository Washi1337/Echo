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
            var result = EvaluateCondition(context, instruction);
            
            // If evaluation failed, we throw an invalid program exception.
            if (result is null)
                return CilDispatchResult.InvalidProgram(context);

            // Otherwise, adjust PC accordingly.
            var value = result.Value;
            switch (value.Value)
            {
                case TrileanValue.False:
                    context.CurrentFrame.ProgramCounter += instruction.Size;
                    break;

                case TrileanValue.True:
                    context.CurrentFrame.ProgramCounter = ((ICilLabel) instruction.Operand!).Offset;
                    break;

                case TrileanValue.Unknown:
                    // TODO: Dispatch to a branch resolver.
                    throw new CilEmulatorException($"Branch condition for {instruction} evaluated in an unknown boolean value.");

                default:
                    throw new ArgumentOutOfRangeException();
            }

            return CilDispatchResult.Success();
        }

        /// <summary>
        /// Evaluates the condition for the branching instruction.
        /// </summary>
        /// <param name="context">The context to evaluate the instruction in.</param>
        /// <param name="instruction">The instruction to dispatch and evaluate.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> otherwise.</returns>
        protected abstract Trilean? EvaluateCondition(CilExecutionContext context, CilInstruction instruction);
    }
}