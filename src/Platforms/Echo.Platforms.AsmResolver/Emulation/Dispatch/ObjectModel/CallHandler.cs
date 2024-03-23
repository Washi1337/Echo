using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Implements a CIL instruction handler for <c>call</c> operations.
    /// </summary>
    [DispatcherTableEntry(CilCode.Call)]
    public class CallHandler : CallHandlerBase
    {
        /// <inheritdoc />
        protected override MethodDevirtualizationResult DevirtualizeMethodInternal(CilExecutionContext context,
            CilInstruction instruction,
            IMethodDescriptor method,
            IList<BitVector> arguments)
        {
            return MethodDevirtualizationResult.Success(method);
        }
    }
}