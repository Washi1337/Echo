using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides a base for all handlers that target fallthrough operation codes.
    /// </summary>
    public abstract class FallThroughOpCodeHandler : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public abstract IReadOnlyCollection<CilCode> SupportedOpCodes
        {
            get;
        }

        /// <inheritdoc />
        public virtual DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            context.ProgramState.ProgramCounter += instruction.Size;
            return DispatchResult.Success();
        }
    }
}