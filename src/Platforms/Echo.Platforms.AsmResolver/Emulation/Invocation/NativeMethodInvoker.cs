using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Implements a method invoker that steps over any method that does not have a CIL method body assigned.
    /// </summary>
    public class NativeMethodInvoker : IMethodInvoker
    {
        /// <summary>
        /// Creates a new native method invoker.
        /// </summary>
        /// <param name="baseInvoker">The invoker that is used when invoking a non-CIL method.</param>
        public NativeMethodInvoker(IMethodInvoker baseInvoker)
        {
            BaseInvoker = baseInvoker;
        }
        
        /// <summary>
        /// Gets the invoker that is used when invoking a non-CIL method.
        /// </summary>
        public IMethodInvoker BaseInvoker
        {
            get;
        }
        
        /// <inheritdoc />
        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            var definition = method.Resolve();
            return definition?.CilMethodBody is null 
                ? BaseInvoker.Invoke(context, method, arguments) 
                : InvocationResult.Inconclusive();
        }
    }
}