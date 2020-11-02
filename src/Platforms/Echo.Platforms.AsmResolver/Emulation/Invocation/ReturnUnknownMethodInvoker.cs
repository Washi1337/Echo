using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Platforms.AsmResolver.Emulation.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides an implementation for an <see cref="IMethodInvoker"/> that always returns an unknown value when the
    /// called method is non-void.
    /// </summary>
    public class ReturnUnknownMethodInvoker : IMethodInvoker
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ReturnUnknownMethodInvoker"/> class.
        /// </summary>
        /// <param name="unknownValueFactory">The factory responsible for constructing the unknown values.</param>
        public ReturnUnknownMethodInvoker(IUnknownValueFactory unknownValueFactory)
        {
            UnknownValueFactory = unknownValueFactory ?? throw new ArgumentNullException(nameof(unknownValueFactory));
        }
        
        /// <summary>
        /// Gets the factory that is responsible for constructing the unknown values. 
        /// </summary>
        public IUnknownValueFactory UnknownValueFactory
        {
            get;
        }

        /// <inheritdoc />
        public IConcreteValue Invoke(IMethodDescriptor method, IEnumerable<IConcreteValue> arguments)
        {
            return CreateReturnValue(method.Signature);
        }

        /// <inheritdoc />
        public IConcreteValue InvokeIndirect(IConcreteValue address, MethodSignature methodSig,
            IEnumerable<IConcreteValue> arguments)
        {
            return CreateReturnValue(methodSig);
        }

        /// <summary>
        /// Creates the return value of a method signature.
        /// </summary>
        /// <param name="methodSig">Method Signature</param>
        /// <returns></returns>
        private IConcreteValue CreateReturnValue(MethodSignatureBase methodSig)
        {
            var returnType = methodSig.ReturnType;
            return returnType.ElementType != ElementType.Void 
                ? UnknownValueFactory.CreateUnknown(returnType)
                : null;
        }
    }
}