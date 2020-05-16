using System;
using AsmResolver.DotNet;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides information about the result of a method devirtualization process.
    /// </summary>
    public readonly struct MethodDevirtualizationResult 
    {
        /// <summary>
        /// Creates a new successful method devirtualization result. 
        /// </summary>
        /// <param name="method">The resolved method.</param>
        public MethodDevirtualizationResult(IMethodDescriptor method)
        {
            ResultingMethod = method ?? throw new ArgumentNullException(nameof(method));
            Exception = null;
        }

        /// <summary>
        /// Creates a new unsuccessful method devirtualization result.
        /// </summary>
        /// <param name="exception">The exception that occurred during the method devirtualization.</param>
        public MethodDevirtualizationResult(Exception exception)
        {
            ResultingMethod = null;
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }
        
        /// <summary>
        /// When successful, gets the resulting devirtualized method.
        /// </summary>
        public IMethodDescriptor ResultingMethod
        {
            get;
        }

        /// <summary>
        /// When unsuccessful, gets the exception thrown during the devirtualization process. 
        /// </summary>
        public Exception Exception
        {
            get;
        }
    }
}