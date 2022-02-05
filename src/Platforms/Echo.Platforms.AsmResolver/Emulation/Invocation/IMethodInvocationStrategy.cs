using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    public interface IMethodInvocationStrategy
    {
        bool ShouldInvoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments);
    }
}