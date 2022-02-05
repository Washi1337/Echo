using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
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
        
        public bool ShouldInvoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            return false;
        }
    }
}