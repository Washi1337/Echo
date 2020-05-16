using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

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
        public ICliValue Invoke(MethodDefinition method, IEnumerable<ICliValue> arguments)
        {
            var returnType = method.Signature.ReturnType;
            return returnType.ElementType != ElementType.Void 
                ? UnknownValueFactory.CreateUnknown(returnType) 
                : null;
        }
    }
}