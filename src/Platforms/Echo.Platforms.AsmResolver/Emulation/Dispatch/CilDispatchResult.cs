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
    }
}