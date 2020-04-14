using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Provides a handler for instructions with the ldc.r8 operation code.
    /// </summary>
    public class LdcR8 : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldc_R8
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            context.ProgramState.Stack.Push(new Float64Value((double) instruction.Operand));
            return base.Execute(context, instruction);
        }
    }
}