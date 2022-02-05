using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    public sealed class InvokeSystemStrategy : IMethodInvocationStrategy
    {
        public static InvokeSystemStrategy Instance
        {
            get;
        } = new();

        private InvokeSystemStrategy()
        {
        }
        
        public bool ShouldInvoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            return KnownCorLibs.KnownCorLibNames.Contains( method.DeclaringType.Scope.Name);
        }
    }
}