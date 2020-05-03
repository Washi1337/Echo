using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

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

        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            bool? result = VerifyCondition(context, instruction);

            int newOffset;
            if (result.HasValue)
            {
                if (result.Value)
                    newOffset = ((ICilLabel) instruction.Operand).Offset;
                else
                    newOffset = instruction.Offset + instruction.Size;
            }
            else
            {
                // TODO: dispatch event, allowing the user to handle unknown branch conditions.
                throw new DispatchException("Branch condition could not be evaluated.");
            }

            context.ProgramState.ProgramCounter = newOffset;
            return DispatchResult.Success();
        }

        /// <summary>
        /// Determines whether the branch condition has been met.
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed in.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        /// <returns><c>true</c> if the branch should be taken, <c>false</c> if not, and <c>null</c> if the conclusion
        /// is unknown.</returns>
        protected abstract bool? VerifyCondition(ExecutionContext context, CilInstruction instruction);
    }
}