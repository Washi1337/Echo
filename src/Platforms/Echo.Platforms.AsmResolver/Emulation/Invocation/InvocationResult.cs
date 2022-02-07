using System.Diagnostics.CodeAnalysis;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Describes the result of an invocation of an external method. 
    /// </summary>
    public readonly struct InvocationResult
    {
        private InvocationResult(bool isSuccess, BitVector? value)
        {
            IsSuccess = isSuccess;
            Value = value;
        }

        /// <summary>
        /// Determines whether the invocation was successful.
        /// </summary>
        [MemberNotNullWhen(false, nameof(Value))]
        public bool IsSuccess
        {
            get;
        }

        /// <summary>
        /// When <see cref="IsSuccess"/> is <c>false</c>, contains the pointer to the exception that was thrown.
        /// Otherwise, contains the value that the method returned (if available).
        /// </summary>
        public BitVector? Value
        {
            get;
        }

        /// <summary>
        /// Constructs a new successful invocation result with the provided return value.
        /// </summary>
        /// <param name="value">The return value, or <c>null</c> if no return value was provided.</param>
        /// <returns>The result.</returns>
        public static InvocationResult Success(BitVector? value) => new(true, value);
        
        /// <summary>
        /// Constructs a new failed invocation result with the provided pointer to an exception object describing the
        /// error that occurred.
        /// </summary>
        /// <param name="value">The pointer to the exception object that was thrown.</param>
        /// <returns>The result.</returns>
        public static InvocationResult Exception(BitVector value) => new(false, value);
    }
}