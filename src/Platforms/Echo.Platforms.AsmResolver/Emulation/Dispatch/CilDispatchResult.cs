using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
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
        /// Creates a new dispatch result indicating the dispatch failed with an exception.
        /// </summary>
        /// <param name="machine">The machine to allocate the exception in.</param>
        /// <param name="type">The type of exception to allocate.</param>
        public static CilDispatchResult Exception(CilVirtualMachine machine, ITypeDescriptor type)
        {
            long exceptionPointer = machine.Heap.AllocateObject(type, true);
            var pointerVector = machine.ValueFactory.BitVectorPool.Rent(machine.Is32Bit ? 32 : 64, false);
            pointerVector.AsSpan().WriteNativeInteger(exceptionPointer, machine.Is32Bit);
            return new CilDispatchResult(pointerVector);
        }

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed due to an invalid program.
        /// </summary>
        /// <param name="context">The context the instruction was evaluated in.</param>
        public static CilDispatchResult InvalidProgram(CilExecutionContext context)
        {
            return Exception(context.Machine, context.Machine.ValueFactory.InvalidProgramExceptionType);
        }

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed due to a null reference.
        /// </summary>
        /// <param name="context">The context the instruction was evaluated in.</param>
        public static CilDispatchResult NullReference(CilExecutionContext context)
        {
            return Exception(context.Machine, context.Machine.ValueFactory.NullReferenceExceptionType);
        }

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed due to an index being out of range.
        /// </summary>
        /// <param name="context">The context the instruction was evaluated in.</param>
        public static CilDispatchResult IndexOutOfRange(CilExecutionContext context)
        {
            return Exception(context.Machine, context.Machine.ValueFactory.IndexOutOfRangeExceptionType);
        }

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed due to a stack overflow.
        /// </summary>
        /// <param name="context">The context the instruction was evaluated in.</param>
        public static CilDispatchResult StackOverflow(CilExecutionContext context)
        {
            return Exception(context.Machine, context.Machine.ValueFactory.StackOverflowExceptionType);
        }

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed due to a stack overflow.
        /// </summary>
        /// <param name="context">The context the instruction was evaluated in.</param>
        /// <param name="originalType">The original object type.</param>
        /// <param name="targetType">The type that the object was attempted to be casted to.</param>
        public static CilDispatchResult InvalidCast(CilExecutionContext context, TypeSignature originalType, TypeSignature targetType)
        {
            // TODO: include originalType and targetType in exception object.

            return Exception(context.Machine, context.Machine.ValueFactory.InvalidCastExceptionType);
        }
    }
}