using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Operators
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Shr"/> and <see cref="CilOpCodes.Shr_Un"/>
    /// operation codes.
    /// </summary>
    public class Shr : ShiftOperator
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Shr, CilCode.Shr_Un
        };
        
        /// <inheritdoc />
        protected override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction,
            IntegerValue value, int shiftCount)
        {
            value.RightShift(shiftCount, instruction.OpCode.Code == CilCode.Shr);
            context.ProgramState.Stack.Push((ICliValue) value);
            return DispatchResult.Success();
        }
    }
}