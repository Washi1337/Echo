using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides members for emulating invocations of external methods.
    /// </summary>
    public interface IMethodInvoker
    {
        /// <summary>
        /// Invokes or emulates an external method.
        /// </summary>
        /// <param name="context">The execution context the call originates from.</param>
        /// <param name="method">The method to invoke.</param>
        /// <param name="arguments">The arguments to invoke the method with.</param>
        /// <returns>The result</returns>
        InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments);
    }
}