using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Constants
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Ldc_R4"/> operation code.
    /// </summary>
    public class LdcR4 : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ldc_R4
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            context.ProgramState.Stack.Push(new FValue((float) instruction.Operand));
            return base.Execute(context, instruction);
        }
    }
}