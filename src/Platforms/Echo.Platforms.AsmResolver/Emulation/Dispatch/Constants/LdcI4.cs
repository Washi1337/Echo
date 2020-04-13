using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Provides a handler for instructions with any variant of the LDC.I4 operation codes.
    /// </summary>
    public class LdcI4 : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldc_I4,
            CilCode.Ldc_I4_0,
            CilCode.Ldc_I4_1,
            CilCode.Ldc_I4_2,
            CilCode.Ldc_I4_3,
            CilCode.Ldc_I4_4,
            CilCode.Ldc_I4_5,
            CilCode.Ldc_I4_6,
            CilCode.Ldc_I4_7,
            CilCode.Ldc_I4_8,
            CilCode.Ldc_I4_M1,
            CilCode.Ldc_I4_S,
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            context.ProgramState.Stack.Push(new Integer32Value(instruction.GetLdcI4Constant()));
            return base.Execute(context, instruction);
        }
    }
}