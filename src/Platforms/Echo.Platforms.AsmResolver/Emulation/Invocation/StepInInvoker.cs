using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    internal sealed class StepInInvoker : IMethodInvoker
    {
        internal static readonly StepInInvoker Instance = new();
        
        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            return InvocationResult.StepIn();
        }
    }
}