using System.Collections.Generic;
using System.Linq;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Invocation;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Invocation
{
    public class InvokerWrapper : IMethodInvoker
    {
        private readonly IMethodInvoker _invoker;

        public InvokerWrapper(IMethodInvoker invoker)
        {
            _invoker = invoker;
        }

        public InvocationResult LastInvocationResult
        {
            get;
            set;
        }

        public IMethodDescriptor? LastMethod
        {
            get;
            set;
        }

        public IList<BitVector>? LastArguments
        {
            get;
            set;
        }

        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            LastMethod = method;
            LastArguments = arguments.Select(x => x.Clone()).ToArray();
            var result = _invoker.Invoke(context, method, arguments);
            LastInvocationResult = result;
            return result;
        }
    }
}