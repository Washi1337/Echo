using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Defines a method that is able to handle an emulated method invocation.
    /// </summary>
    public delegate InvocationResult MethodHandler(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments);
    
    /// <summary>
    /// Wraps a <see cref="MethodHandler"/> delegate into a <see cref="IMethodInvoker"/>.
    /// </summary>
    public class CallbackMethodInvoker : IMethodInvoker
    {
        private readonly MethodHandler _handler;

        /// <summary>
        /// Wraps a <see cref="MethodHandler"/> delegate into a <see cref="IMethodInvoker"/>.
        /// </summary>
        /// <param name="handler">The delegate to wrap.</param>
        public CallbackMethodInvoker(MethodHandler handler)
        {
            _handler = handler;
        }

        /// <inheritdoc />
        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            return _handler(context, method, arguments);
        }
    }
}