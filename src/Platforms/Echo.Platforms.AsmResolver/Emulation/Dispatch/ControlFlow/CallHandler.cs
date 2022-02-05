using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
{
    [DispatcherTableEntry(CilCode.Call)]
    public class CallHandler : CallHandlerBase
    {
        protected override MethodDevirtualizationResult DevirtualizeMethod(CilExecutionContext context,
            CilInstruction instruction, IList<BitVector> arguments)
        {
            return new MethodDevirtualizationResult((IMethodDescriptor) instruction.Operand!);
        }
    }
}