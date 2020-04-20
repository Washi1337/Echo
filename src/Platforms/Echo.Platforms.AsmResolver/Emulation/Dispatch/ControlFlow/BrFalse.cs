using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Brfalse"/> and <see cref="CilOpCodes.Brfalse_S"/>
    /// operation codes.
    /// </summary>
    public class BrFalse : BranchHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Brfalse, CilCode.Brfalse_S
        };

        /// <inheritdoc />
        protected override bool? VerifyCondition(ExecutionContext context, CilInstruction instruction) => 
            context.ProgramState.Stack.Pop().IsZero;
    }
}