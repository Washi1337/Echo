using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Miscellaneous
{
    /// <summary>
    /// Provides a handler for instructions with the DUP operation code.
    /// </summary>
    public class Dup : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Dup
        };

        /// <inheritdoc />
        public override DispatchResult Execute(CilExecutionContext context, CilInstruction instruction)
        {
            var stack = context.ProgramState.Stack;
            stack.Push((ICliValue) stack.Top.Copy());
            return base.Execute(context, instruction);
        }
    }
}