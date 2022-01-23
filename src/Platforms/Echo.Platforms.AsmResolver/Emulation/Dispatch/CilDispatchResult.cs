using System.Diagnostics.CodeAnalysis;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides information about the result of an instruction dispatch.
    /// </summary>
    public readonly struct CilDispatchResult
    {
        private CilDispatchResult(BitVector? exceptionPointer)
        {
            ExceptionPointer = exceptionPointer;
        }
        
        /// <summary>
        /// Gets a value indicating whether the dispatch and evaluation of the instruction was successful. 
        /// </summary>
        [MemberNotNullWhen(false, nameof(ExceptionPointer))]
        public bool IsSuccess => ExceptionPointer is null;

        /// <summary>
        /// When <see cref="IsSuccess"/> is <c>false</c>, gets a vector that represents the pointer to the exception
        /// that was thrown during the evaluation of the instruction.
        /// </summary>
        public BitVector? ExceptionPointer
        {
            get;
        }

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch was successful.
        /// </summary>
        public static CilDispatchResult Success() => new();

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed with an exception.
        /// </summary>
        /// <param name="exceptionPointer">The pointer to the exception that was thrown.</param>
        public static CilDispatchResult Exception(BitVector exceptionPointer) => new(exceptionPointer);

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed due to an invalid program.
        /// </summary>
        /// <param name="context">The context the instruction was evaluated in.</param>
        public static CilDispatchResult InvalidProgram(CilExecutionContext context)
        {
            long exceptionPointer = context.Machine.Heap.AllocateObject(context.Machine.ValueFactory.InvalidProgramExceptionType, true);
            var pointerVector = context.Machine.ValueFactory.BitVectorPool.Rent(context.Machine.Is32Bit ? 32 : 64, false);
            pointerVector.AsSpan().WriteNativeInteger(0, exceptionPointer, context.Machine.Is32Bit);
            return new CilDispatchResult(pointerVector);
        }
    }
}