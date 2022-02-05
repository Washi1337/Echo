using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    public sealed class AlwaysInvokeStrategy : IMethodInvocationStrategy
    {
        public static AlwaysInvokeStrategy Instance
        {
            get;
        } = new();

        private AlwaysInvokeStrategy()
        {
        }

        public bool ShouldInvoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            return true;
        }
    }
}