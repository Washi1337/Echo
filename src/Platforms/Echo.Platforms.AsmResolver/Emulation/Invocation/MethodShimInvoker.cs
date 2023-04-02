using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides an implementation that handles invocations on a per-method basis.  
    /// </summary>
    public class MethodShimInvoker : IMethodInvoker
    {
        private readonly IDictionary<IMethodDescriptor, IMethodInvoker> _handlers;
        
        /// <summary>
        /// Creates a new method shim invoker.
        /// </summary>
        public MethodShimInvoker()
            : this(SignatureComparer.Default)
        {
        }

        /// <summary>
        /// Creates a new method shim invoker.
        /// </summary>
        /// <param name="comparer">The comparer object to use when comparing methods.</param>
        public MethodShimInvoker(SignatureComparer comparer)
        {
            _handlers = new Dictionary<IMethodDescriptor, IMethodInvoker>(comparer);
        }
        
        /// <summary>
        /// Assigns a method invoker to a specific method.
        /// </summary>
        /// <param name="method">The method to handle.</param>
        /// <param name="handler">The invoker that will handle the method.</param>
        /// <returns>The current method shim invoker.</returns>
        public MethodShimInvoker Map(IMethodDescriptor method, IMethodInvoker handler)
        {
            _handlers[method] = handler;
            return this;
        }

        /// <summary>
        /// Assigns a method invoker to a specific method.
        /// </summary>
        /// <param name="method">The method to handle.</param>
        /// <param name="handler">The invoker that will handle the method.</param>
        /// <returns>The current method shim invoker.</returns>
        public MethodShimInvoker Map(IMethodDescriptor method, MethodHandler handler)
        {
            _handlers[method] = new CallbackMethodInvoker(handler);
            return this;
        }

        /// <summary>
        /// Assigns a methods invoker to multiple methods.
        /// </summary>
        /// <param name="methods">The methods to handle.</param>
        /// <param name="handler">The invoker that will handle the method.</param>
        /// <returns>The current method shim invoker.</returns>
        public MethodShimInvoker MapMany(IEnumerable<IMethodDescriptor> methods, IMethodInvoker handler)
        {
            foreach (var method in methods)
                _handlers[method] = handler;

            return this;
        }

        /// <summary>
        /// Assigns a methods invoker to multiple methods.
        /// </summary>
        /// <param name="methods">The methods to handle.</param>
        /// <param name="handler">The invoker that will handle the method.</param>
        /// <returns>The current method shim invoker.</returns>
        public MethodShimInvoker MapMany(IEnumerable<IMethodDescriptor> methods, MethodHandler handler)
        {
            var invoker = new CallbackMethodInvoker(handler);
            foreach (var method in methods)
                _handlers[method] = invoker;

            return this;
        }

        /// <inheritdoc />
        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            return _handlers.TryGetValue(method, out var handler) 
                ? handler.Invoke(context, method, arguments) 
                : InvocationResult.Inconclusive();
        }
    }
}