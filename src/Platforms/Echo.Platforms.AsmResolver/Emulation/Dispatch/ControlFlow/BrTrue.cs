using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Core;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    /// <summary>
    /// Provides a handler for instructions with the <see cref="CilOpCodes.Brtrue"/> and <see cref="CilOpCodes.Brtrue_S"/>
    /// operation codes.
    /// </summary>
    public class BrTrue : BranchHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Brtrue, CilCode.Brtrue_S
        };

        /// <inheritdoc />
        public override Trilean VerifyCondition(ExecutionContext context, CilInstruction instruction) => 
            context.ProgramState.Stack.Top.IsNonZero;
    }
}