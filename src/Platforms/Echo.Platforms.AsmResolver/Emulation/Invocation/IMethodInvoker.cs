using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    public interface IMethodInvoker
    {
        BitVector? Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments);
    }
}