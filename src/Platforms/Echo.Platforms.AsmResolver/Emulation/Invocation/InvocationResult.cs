
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Describes the result of an invocation of an external method. 
    /// </summary>
    public readonly struct InvocationResult
    {
        private InvocationResult(InvocationResultType resultType, BitVector? value)
        {
            ResultType = resultType;
            Value = value;
        }

        /// <summary>
        /// Gets the type of result this object contains. 
        /// </summary>
        public InvocationResultType ResultType
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the invocation was inconclusive and not handled yet.
        /// </summary>
        public bool IsInconclusive => ResultType is InvocationResultType.Inconclusive;

        /// <summary>
        /// Determines whether the invocation was successful.
        /// </summary>
        public bool IsSuccess => ResultType is InvocationResultType.StepIn or InvocationResultType.StepOver;

        /// <summary>
        /// When <see cref="IsSuccess"/> is <c>false</c>, contains the pointer to the exception that was thrown.
        /// Otherwise, contains the value that the method returned (if available).
        /// </summary>
        public BitVector? Value
        {
            get;
        }
        
        /// <summary>
        /// Constructs a new inconclusive invocation result, where the invocation was not handled yet.
        /// </summary>
        public static InvocationResult Inconclusive() => new(InvocationResultType.Inconclusive, null);
        
        /// <summary>
        /// Constructs a new inconclusive invocation result, where the invocation was handled as a step-in action.
        /// </summary>
        public static InvocationResult StepIn() => new(InvocationResultType.StepIn, null);
        
        /// <summary>
        /// Constructs a new inconclusive invocation result, where the invocation was fully handled by the invoker and
        /// a result was produced.
        /// </summary>
        /// <param name="value">
        /// The result that was produced by the method, or <c>null</c> if the method does not return a value.
        /// </param>
        public static InvocationResult StepOver(BitVector? value) => new(InvocationResultType.StepOver, value);
        
        /// <summary>
        /// Constructs a new failed invocation result with the provided pointer to an exception object describing the
        /// error that occurred.
        /// </summary>
        /// <param name="value">The pointer to the exception object that was thrown.</param>
        /// <returns>The result.</returns>
        public static InvocationResult Exception(BitVector value) => new(InvocationResultType.Exception, value);
    }

}