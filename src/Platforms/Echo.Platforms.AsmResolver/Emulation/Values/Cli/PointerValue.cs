using Echo.Concrete.Values.ReferenceType;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values.Cli
{
    /// <summary>
    /// Represents a pointer on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public class PointerValue : RelativePointerValue, ICliValue
    {
        /// <summary>
        /// Creates a new null pointer value.
        /// </summary>
        /// <param name="isKnown">Indicates whether the pointer is known.</param>
        public PointerValue(bool isKnown)
            : base(isKnown)
        {
        }

        /// <summary>
        /// Creates a new pointer value.
        /// </summary>
        /// <param name="basePointer">The base pointer value.</param>
        public PointerValue(IPointerValue basePointer)
            : base(basePointer)
        {
        }

        /// <summary>
        /// Creates a new pointer value.
        /// </summary>
        /// <param name="basePointer">The base pointer value.</param>
        /// <param name="offset">The offset relative to the base pointer.</param>
        public PointerValue(IPointerValue basePointer, int offset)
            : base(basePointer, offset)
        {
        }

        /// <inheritdoc />
        public CliValueType CliValueType => CliValueType.UnmanagedPointer;

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsI(bool is32Bit) => new NativeIntegerValue(0, 0, is32Bit);

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsU(bool is32Bit) => new NativeIntegerValue(0, 0, is32Bit);

        /// <inheritdoc />
        public I4Value InterpretAsI1() => new I4Value(0, 0xFFFFFF00);
        
        /// <inheritdoc />
        public I4Value InterpretAsU1() => new I4Value(0, 0xFFFFFF00);

        /// <inheritdoc />
        public I4Value InterpretAsI2() => new I4Value(0, 0xFFFF0000);

        /// <inheritdoc />
        public I4Value InterpretAsU2() => new I4Value(0, 0xFFFF0000);

        /// <inheritdoc />
        public I4Value InterpretAsI4() => new I4Value(0, 0);

        /// <inheritdoc />
        public I4Value InterpretAsU4() => new I4Value(0, 0);

        /// <inheritdoc />
        public I8Value InterpretAsI8() => new I8Value(0, 0);

        /// <inheritdoc />
        public FValue InterpretAsR4() => new FValue(0);

        /// <inheritdoc />
        public FValue InterpretAsR8() => new FValue(0);

        /// <inheritdoc />
        public OValue InterpretAsRef(bool is32Bit) => new OValue(null, false, is32Bit);

        /// <inheritdoc />
        public NativeIntegerValue ConvertToI(bool is32Bit, bool unsigned, out bool overflowed) => InterpretAsI(is32Bit);

        /// <inheritdoc />
        public NativeIntegerValue ConvertToU(bool is32Bit, bool unsigned, out bool overflowed) => InterpretAsU(is32Bit);

        /// <inheritdoc />
        public I4Value ConvertToI1(bool unsigned, out bool overflowed) => InterpretAsI1();

        /// <inheritdoc />
        public I4Value ConvertToU1(bool unsigned, out bool overflowed) => InterpretAsU1();

        /// <inheritdoc />
        public I4Value ConvertToI2(bool unsigned, out bool overflowed) => InterpretAsI2();

        /// <inheritdoc />
        public I4Value ConvertToU2(bool unsigned, out bool overflowed) => InterpretAsU2();

        /// <inheritdoc />
        public I4Value ConvertToI4(bool unsigned, out bool overflowed) => InterpretAsI4();

        /// <inheritdoc />
        public I4Value ConvertToU4(bool unsigned, out bool overflowed) => InterpretAsU4();

        /// <inheritdoc />
        public I8Value ConvertToI8(bool unsigned, out bool overflowed) => InterpretAsI8();

        /// <inheritdoc />
        public I8Value ConvertToU8(bool unsigned, out bool overflowed) => InterpretAsI8();

        /// <inheritdoc />
        public FValue ConvertToR4() => InterpretAsR4();

        /// <inheritdoc />
        public FValue ConvertToR8() => InterpretAsR8();

        /// <inheritdoc />
        public FValue ConvertToR() => InterpretAsR8();

        /// <inheritdoc />
        public IValue Copy() => new PointerValue(BasePointer, CurrentOffset);
    }
}