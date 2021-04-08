using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Shl"/> operation code.
    /// </summary>
    public class Shl : ShiftOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Shl
        };
        
        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction,
            IntegerValue value, int shiftCount)
        {
            value.LeftShift(shiftCount);
            context.ProgramState.Stack.Push((ICliValue) value);
            return DispatchResult.Success();
        }
    }
}