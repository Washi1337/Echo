using System;
using AsmResolver.DotNet.Collections;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver
{
    /// <summary>
    /// Represents a parameter that is declared and can be referenced within a CIL method.
    /// </summary>
    public class CilParameter : IVariable
    {
        /// <summary>
        /// Creates a new CIL parameter.
        /// </summary>
        /// <param name="parameter">The underlying parameter</param>
        public CilParameter(Parameter parameter)
        {
            Parameter = parameter ?? throw new ArgumentNullException(nameof(parameter));
        }
        
        /// <summary>
        /// Gets the underlying parameter object.
        /// </summary>
        public Parameter Parameter
        {
            get;
        }

        /// <inheritdoc />
        public string Name => Parameter.Name;
    }
}