using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Invocation;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch
{
    public sealed class NeverInvokeStrategy : IMethodInvocationStrategy
    {
        public static NeverInvokeStrategy Instance
        {
            get;
        } = new();
        
        private NeverInvokeStrategy()
        {
        }
        
        public bool ShouldInvoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments) => false;
    }
}