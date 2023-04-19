using System.Diagnostics;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides information about the result of an instruction dispatch.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public readonly struct CilDispatchResult
    {
        private CilDispatchResult(ObjectHandle exceptionObject)
        {
            ExceptionObject = exceptionObject;
        }
        
        /// <summary>
        /// Gets a value indicating whether the dispatch and evaluation of the instruction was successful. 
        /// </summary>
        public bool IsSuccess => ExceptionObject.IsNull;

        /// <summary>
        /// When <see cref="IsSuccess"/> is <c>false</c>, gets the o that represents the pointer to the exception
        /// that was thrown during the evaluation of the instruction.
        /// </summary>
        public ObjectHandle ExceptionObject
        {
            get;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal object DebuggerDisplay => IsSuccess ? "Success" : ExceptionObject;

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch was successful.
        /// </summary>
        public static CilDispatchResult Success() => new();

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed with an exception.
        /// </summary>
        /// <param name="exceptionObject">The handle to the exception object that was thrown.</param>
        public static CilDispatchResult Exception(ObjectHandle exceptionObject) => new(exceptionObject);

        /// <summary>
        /// Creates a new dispatch result indicating the dispatch failed with an exception.
        /// </summary>
        /// <param name="machine">The machine to allocate the exception in.</param>
        /// <param name="type">The type of exception to allocate.</param>
        public static CilDispatchResult Exception(CilVirtualMachine machine, ITypeDescriptor type)
        {
            var exceptionObject = machine.Heap.AllocateObject(type, true).AsObjectHandle(machine);
            return new CilDispatchResult(exceptionObject);
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
        /// Creates a new dispatch result indicating the dispatch failed due to an integer overflow.
        /// </summary>
        /// <param name="context">The context the instruction was evaluated in.</param>
        public static CilDispatchResult Overflow(CilExecutionContext context)
        {
            return Exception(context.Machine, context.Machine.ValueFactory.OverflowExceptionType);
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