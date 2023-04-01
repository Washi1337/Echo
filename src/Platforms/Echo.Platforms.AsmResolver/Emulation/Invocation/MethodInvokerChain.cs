using System.Collections.Generic;
using System.Diagnostics;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Represents a chain of method invokers that are invoked in sequence until one of the invokers produces a
    /// conclusive result.
    /// </summary>
    [DebuggerDisplay("Count = {Invokers.Count}")]
    public class MethodInvokerChain : IMethodInvoker
    {
        /// <summary>
        /// Gets the list of invokers to be called.
        /// </summary>
        public IList<IMethodInvoker> Invokers
        {
            get;
        } = new List<IMethodInvoker>();

        /// <inheritdoc />
        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            for (int i = 0; i < Invokers.Count; i++)
            {
                var result = Invokers[i].Invoke(context, method, arguments);
                if (!result.IsInconclusive)
                    return result;
            }

            return InvocationResult.Inconclusive();
        }
    }
}