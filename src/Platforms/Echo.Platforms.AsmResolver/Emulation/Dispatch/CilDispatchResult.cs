using System.Diagnostics.CodeAnalysis;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    public readonly struct CilDispatchResult
    {
        public CilDispatchResult(BitVector? exceptionPointer)
        {
            ExceptionPointer = exceptionPointer;
        }

        [MemberNotNullWhen(false, nameof(ExceptionPointer))]
        public bool IsSuccess => ExceptionPointer is null;

        public BitVector? ExceptionPointer
        {
            get;
        }

        public static CilDispatchResult Success() => new();

        public static CilDispatchResult Exception(BitVector exceptionPointer) => new(exceptionPointer);

        public static CilDispatchResult InvalidProgram(CilExecutionContext context)
        {
            long exceptionPointer = context.Machine.Heap.AllocateObject(context.Machine.ValueFactory.InvalidProgramExceptionType, true);
            var pointerVector = context.Machine.ValueFactory.BitVectorPool.Rent(context.Machine.Is32Bit ? 32 : 64, false);
            pointerVector.AsSpan().WriteNativeInteger(0, exceptionPointer, context.Machine.Is32Bit);
            return new CilDispatchResult(pointerVector);
        }
    }
}