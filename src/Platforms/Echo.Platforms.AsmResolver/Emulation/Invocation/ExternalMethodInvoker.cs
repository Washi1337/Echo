using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Implements a method invoker that steps over any method that is defined outside of the current method's
    /// resolution scope.
    /// </summary>
    public class ExternalMethodInvoker : IMethodInvoker
    {
        private readonly SignatureComparer _comparer;
        
        /// <summary>
        /// Creates a new step-over external method invoker.
        /// </summary>
        /// <param name="baseInvoker">The invoker to use when stepping over.</param>
        /// <param name="comparer"></param>
        public ExternalMethodInvoker(IMethodInvoker baseInvoker, SignatureComparer comparer)
        {
            _comparer = comparer;
            BaseInvoker = baseInvoker;
        }

        /// <summary>
        /// Gets the invoker that is used when invoking an external method.
        /// </summary>
        public IMethodInvoker BaseInvoker
        {
            get;
        }

        /// <inheritdoc />
        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            var scope1 = method.DeclaringType?.Scope;
            var scope2 = context.CurrentFrame.Method.DeclaringType?.Scope;

            bool isExternalCall = scope1 is null || scope2 is null || !_comparer.Equals(scope1, scope2);

            return isExternalCall 
                ? BaseInvoker.Invoke(context, method, arguments) 
                : InvocationResult.Inconclusive();
        }
    }
}