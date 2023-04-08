using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch.ObjectModel
{
    /// <summary>
    /// Provides information about the result of a method devirtualization process.
    /// </summary>
    public readonly struct MethodDevirtualizationResult 
    {
        private MethodDevirtualizationResult(IMethodDescriptor? method, ObjectHandle exceptionObject)
        {
            ResultingMethod = method;
            ExceptionObject = exceptionObject;
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
        public ObjectHandle ExceptionObject
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the devirtualization process of the referenced method was successful.
        /// </summary>
        [MemberNotNullWhen(true, nameof(ResultingMethod))]
        public bool IsSuccess => ResultingMethod is not null;

        /// <summary>
        /// Gets a value indicating whether the devirtualization process could not be completed due to an unknown
        /// object that was dereferenced.
        /// </summary>
        public bool IsUnknown => !IsSuccess && ExceptionObject.IsNull;

        /// <summary>
        /// Creates a new successful method devirtualization result. 
        /// </summary>
        /// <param name="method">The resolved method.</param>
        public static MethodDevirtualizationResult Success(IMethodDescriptor method) => new(method, default);

        /// <summary>
        /// Creates a new unsuccessful method devirtualization result.
        /// </summary>
        /// <param name="exceptionObject">
        /// The handle to the exception that occurred during the method devirtualization.
        /// </param>
        public static MethodDevirtualizationResult Exception(ObjectHandle exceptionObject) => new(null, exceptionObject);

        /// <summary>
        /// Creates a new unknown method devirtualization result.
        /// </summary>
        public static MethodDevirtualizationResult Unknown() => new(null, default);
    }
}