using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Miscellaneous
{
    /// <summary>
    /// Provides a handler for instructions with the POP operation code.
    /// </summary>
    public class Pop : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Pop
        };

        /// <inheritdoc />
        public override DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            context.ProgramState.Stack.Pop();
            return base.Execute(context, instruction);
        }
    }
}