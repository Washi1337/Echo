using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides members for deciding whether a method call should be invoked or stepped into.
    /// </summary>
    public interface IMethodInvocationStrategy
    {
        /// <summary>
        /// Determines whether the provided method should be invoked or not.
        /// </summary>
        /// <param name="context">The execution context the call originates from.</param>
        /// <param name="method">The method to invoke.</param>
        /// <param name="arguments">The arguments to invoke the method with.</param>
        /// <returns>
        /// <c>true</c> if the method should be invoked by a <see cref="IMethodInvoker"/>,
        /// <c>false</c> if the method should be stepped into by the virtual machine instead.
        /// </returns>
        bool ShouldInvoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments);
    }
}