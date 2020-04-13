using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;

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
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var stack = context.ProgramState.Stack;
            stack.Push((IConcreteValue) stack.Top.Copy());
            return base.Execute(context, instruction);
        }
    }
}