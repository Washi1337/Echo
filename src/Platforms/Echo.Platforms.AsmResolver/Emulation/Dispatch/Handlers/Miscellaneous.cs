using System.Collections.Generic;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.Handlers
{
    /// <summary>
    /// Provides a handler for instructions with the NOP operation code.
    /// </summary>
    public class Nop : FallThroughOpCodeHandler
    {
        /// <inheritdoc />
        public override IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Nop
        };
    }
    
    /// <summary>
    /// Provides a handler for instructions with the RET operation code.
    /// </summary>
    public class Ret : ICilOpCodeHandler
    {
        /// <inheritdoc />
        public IReadOnlyCollection<CilCode> SupportedOpCodes => new[]
        {
            CilCode.Ret
        };
        
        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            return new DispatchResult
            {
                Exit = true
            };
        }
    }
}