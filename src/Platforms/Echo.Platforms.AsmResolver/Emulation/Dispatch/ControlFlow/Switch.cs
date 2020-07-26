using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Switch"/>  operation code.
    /// </summary>
    public class Switch : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Switch
        };

        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var stack = context.ProgramState.Stack;

            // Pop index value.
            var indexValue = stack.Pop();
            
            // Check if known.
            if (!indexValue.IsKnown)
            {
                // TODO: dispatch event, allowing the user to handle unknown branch conditions.
                throw new DispatchException("Switch branch index could not be evaluated.");
            }
            
            // Read actual index.
            if (!(indexValue is I4Value { U32: uint index }))
                return DispatchResult.InvalidProgram();

            // Jump to switch offset if possible, else take default branch.
            var offsetTable = (IList<ICilLabel>) instruction.Operand;
            context.ProgramState.ProgramCounter = index < offsetTable.Count
                ? offsetTable[(int) index].Offset
                : instruction.Offset + instruction.Size;

            return DispatchResult.Success();
        }
    }
}