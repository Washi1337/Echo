using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides an invocation strategy that always decides on invoking an external call.
    /// </summary>
    public sealed class AlwaysInvokeStrategy : IMethodInvocationStrategy
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="AlwaysInvokeStrategy"/> class.
        /// </summary>
        public static AlwaysInvokeStrategy Instance
        {
            get;
        } = new();

        private AlwaysInvokeStrategy()
        {
        }

        /// <inheritdoc />
        public bool ShouldInvoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            return true;
        }
    }
}