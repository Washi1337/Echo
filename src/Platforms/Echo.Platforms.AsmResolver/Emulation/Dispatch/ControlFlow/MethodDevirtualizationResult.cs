using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ControlFlow
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
            ExceptionPointer = null;
        }

        /// <summary>
        /// Creates a new unsuccessful method devirtualization result.
        /// </summary>
        /// <param name="exception">The exception that occurred during the method devirtualization.</param>
        public MethodDevirtualizationResult(long? exception)
        {
            ResultingMethod = null;
            ExceptionPointer = exception ?? throw new ArgumentNullException(nameof(exception));
        }
        
        /// <summary>
        /// When successful, gets the resulting devirtualized method.
        /// </summary>
        public IMethodDescriptor? ResultingMethod
        {
            get;
        }
        

        /// <summary>
        /// When unsuccessful, gets the exception thrown during the devirtualization process. 
        /// </summary>
        public long? ExceptionPointer
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the devirtualization process of the referenced method was successful.
        /// </summary>
        [MemberNotNullWhen(true, nameof(ResultingMethod))]
        [MemberNotNullWhen(false, nameof(ExceptionPointer))]
        public bool IsSuccess => ResultingMethod is not null;

        /// <summary>
        /// Gets a value indicating whether the devirtualization process could not be completed due to an unknown
        /// object that was dereferenced.
        /// </summary>
        public bool IsUnknown => !IsSuccess && ExceptionPointer is null;
    }
}