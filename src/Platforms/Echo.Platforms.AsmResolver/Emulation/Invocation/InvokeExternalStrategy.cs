using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides an invocation strategy that decides on invoking an external call based on whether the method is
    /// within a different module than the caller's frame.
    /// </summary>
    public sealed class InvokeExternalStrategy : IMethodInvocationStrategy
    {
        private static readonly SignatureComparer Comparer = new();
        
        /// <summary>
        /// Gets the singleton instance of the <see cref="InvokeExternalStrategy"/> class.
        /// </summary>
        public static InvokeExternalStrategy Instance
        {
            get;
        } = new();

        private InvokeExternalStrategy()
        {
        }

        /// <inheritdoc />
        public bool ShouldInvoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            return !Comparer.Equals(
                method.DeclaringType?.Scope,
                context.CurrentFrame.Method.DeclaringType?.Scope);
        }
    }
}