using System;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Values.Cli
{
    /// <summary>
    /// Represents a structure on the evaluation stack of the Common Language Infrastructure (CLI).
    /// </summary>
    public class StructValue : LleStructValue, ICliValue
    {
        /// <summary>
        /// Creates a new structure value.
        /// </summary>
        /// <param name="valueFactory">The object responsible for memory management in the virtual machine.</param>
        /// <param name="valueType">The type of the object.</param>
        /// <param name="contents">The raw contents of the structure.</param>
        public StructValue(IValueFactory valueFactory, TypeSignature valueType, MemoryBlockValue contents)
            : base(valueFactory, valueType, contents)
        {
        }

        /// <inheritdoc />
        public CliValueType CliValueType => CliValueType.Structure;

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsI(bool is32Bit)
        {
            return new NativeIntegerValue(is32Bit
                    ? (IntegerValue) ReadInteger32(0)
                    : ReadInteger64(0),
                is32Bit);
        }

        /// <inheritdoc />
        public NativeIntegerValue InterpretAsU(bool is32Bit)
        {
            return new NativeIntegerValue(is32Bit
                    ? (IntegerValue) ReadInteger32(0)
                    : ReadInteger64(0),
                is32Bit);
        }

        /// <inheritdoc />
        public I4Value InterpretAsI1()
        {
            var result = ReadInteger8(0);
            return new I4Value(result.I8, result.Mask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU1()
        {
            var result = ReadInteger8(0);
            return new I4Value(result.U8, result.Mask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsI2()
        {
            var result = ReadInteger16(0);
            return new I4Value(result.I16, result.Mask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU2()
        {
            var result = ReadInteger16(0);
            return new I4Value(result.U16, result.Mask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsI4()
        {
            var result = ReadInteger32(0);
            return new I4Value(result.I32, result.Mask);
        }

        /// <inheritdoc />
        public I4Value InterpretAsU4()
        {
            var result = ReadInteger32(0);
            return new I4Value(result.I32, result.Mask);
        }

        /// <inheritdoc />
        public I8Value InterpretAsI8()
        {
            var result = ReadInteger64(0);
            return new I8Value(result.I64, result.Mask);
        }

        /// <inheritdoc />
        public FValue InterpretAsR4() => new FValue(ReadFloat32(0).F32);

        /// <inheritdoc />
        public FValue InterpretAsR8() => new FValue(ReadFloat64(0).F64);

        /// <inheritdoc />
        public OValue InterpretAsRef(bool is32Bit) => new OValue(null, true, is32Bit);

        /// <inheritdoc />
        public NativeIntegerValue ConvertToI(bool is32Bit, bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToI(is32Bit, unsigned, out overflowed);

        /// <inheritdoc />
        public NativeIntegerValue ConvertToU(bool is32Bit, bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToU(is32Bit, unsigned, out overflowed);

        /// <inheritdoc />
        public I4Value ConvertToI1(bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToI1(unsigned, out overflowed);

        /// <inheritdoc />
        public I4Value ConvertToU1(bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToU1(unsigned, out overflowed);

        /// <inheritdoc />
        public I4Value ConvertToI2(bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToI2(unsigned, out overflowed);

        /// <inheritdoc />
        public I4Value ConvertToU2(bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToU2(unsigned, out overflowed);

        /// <inheritdoc />
        public I4Value ConvertToI4(bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToI4(unsigned, out overflowed);

        /// <inheritdoc />
        public I4Value ConvertToU4(bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToU4(unsigned, out overflowed);

        /// <inheritdoc />
        public I8Value ConvertToI8(bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToI8(unsigned, out overflowed);

        /// <inheritdoc />
        public I8Value ConvertToU8(bool unsigned, out bool overflowed) => 
            InterpretAsPrimitive().ConvertToU8(unsigned, out overflowed);

        /// <inheritdoc />
        public FValue ConvertToR4() => InterpretAsPrimitive().ConvertToR4();

        /// <inheritdoc />
        public FValue ConvertToR8() => InterpretAsPrimitive().ConvertToR8();

        /// <inheritdoc />
        public FValue ConvertToR() => InterpretAsPrimitive().ConvertToR();

        private ICliValue InterpretAsPrimitive()
        {
            switch (Type.ElementType)
            {
                case ElementType.I1:
                    return InterpretAsI1();

                case ElementType.Boolean:
                case ElementType.U1:
                    return InterpretAsU1();

                case ElementType.I2:
                    return InterpretAsI2();

                case ElementType.Char:
                case ElementType.U2:
                    return InterpretAsU2();

                case ElementType.I4:
                    return InterpretAsI4();

                case ElementType.U4:
                    return InterpretAsU4();

                case ElementType.I8:
                case ElementType.U8:
                    return InterpretAsI8();

                case ElementType.R4:
                    return InterpretAsR4();

                case ElementType.R8:
                    return InterpretAsR8();

                case ElementType.I:
                    return InterpretAsI(Is32Bit);

                case ElementType.U:
                    return InterpretAsU(Is32Bit);

                default:
                    throw new ArgumentException("Structure is not a primitive.");
            }
        }
        
    }
}