using System;
using AsmResolver.DotNet.Signatures;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using AsmResElementType = AsmResolver.PE.DotNet.Metadata.Tables.Rows.ElementType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents an array that contains values that are inheriting from <see cref="ValueType"/>, and are passed on by
    /// value rather than by reference.
    /// </summary>
    public class ValueTypeArrayValue : IDotNetArrayValue
    {
        private readonly MemoryPointerValue _contents;

        /// <summary>
        /// Creates a new value type array. 
        /// </summary>
        /// <param name="elementType">The element type of the array.</param>
        /// <param name="contents">The raw contents of the array.</param>
        public ValueTypeArrayValue(TypeSignature elementType, MemoryPointerValue contents)
        {
            ElementType = elementType;
            _contents = contents;

            // TODO: replace length calculation with future AsmResolver GetSize methods.
            int elementSize = elementType.ElementType switch
            {
                AsmResElementType.Boolean => 4,
                AsmResElementType.Char => 2,
                AsmResElementType.I1 => 1,
                AsmResElementType.U1 => 1,
                AsmResElementType.I2 => 2,
                AsmResElementType.U2 => 2,
                AsmResElementType.I4 => 4,
                AsmResElementType.U4 => 4,
                AsmResElementType.I8 => 8,
                AsmResElementType.U8 => 8,
                AsmResElementType.R4 => 4,
                AsmResElementType.R8 => 8,
                AsmResElementType.I => contents.Is32Bit ? 4 : 8,
                AsmResElementType.U => contents.Is32Bit ? 4 : 8,
                _ => throw new ArgumentOutOfRangeException()
            };

            Length = _contents.Length / elementSize;
        }

        /// <inheritdoc />
        public TypeSignature ElementType
        {
            get;
        }

        private CorLibTypeFactory CorLibTypeFactory => ElementType.Module.CorLibTypeFactory;

        /// <inheritdoc />
        public int Length
        {
            get;
        }

        /// <inheritdoc />
        public bool IsKnown => _contents.IsKnown;

        /// <inheritdoc />
        public int Size => _contents.Size;

        /// <inheritdoc />
        public IValue Copy() => new ValueTypeArrayValue(ElementType, _contents);

        /// <inheritdoc />
        public bool IsValueType => false;

        /// <inheritdoc />
        public bool? IsZero => false;

        /// <inheritdoc />
        public bool? IsNonZero => true;

        /// <inheritdoc />
        public bool? IsPositive => true;

        /// <inheritdoc />
        public bool? IsNegative => false;

        /// <inheritdoc />
        public I4Value LoadElementI1(int index, ICliMarshaller marshaller)
        {
            return (I4Value) marshaller.ToCliValue(
                _contents.ReadInteger8(index * sizeof(sbyte)),
                CorLibTypeFactory.SByte);
        }

        /// <inheritdoc />
        public I4Value LoadElementI2(int index, ICliMarshaller marshaller)
        {
            return (I4Value) marshaller.ToCliValue(
                _contents.ReadInteger16(index * sizeof(short)),
                CorLibTypeFactory.Int16);
        }

        /// <inheritdoc />
        public I4Value LoadElementI4(int index, ICliMarshaller marshaller)
        {
            return (I4Value) marshaller.ToCliValue(
                _contents.ReadInteger32(index * sizeof(int)),
                CorLibTypeFactory.Int32);
        }

        /// <inheritdoc />
        public I8Value LoadElementI8(int index, ICliMarshaller marshaller)
        {
            return (I8Value) marshaller.ToCliValue(
                _contents.ReadInteger64(index * sizeof(long)),
                CorLibTypeFactory.Int64);
        }

        /// <inheritdoc />
        public I4Value LoadElementU1(int index, ICliMarshaller marshaller)
        {
            return (I4Value) marshaller.ToCliValue(
                _contents.ReadInteger8(index * sizeof(byte)),
                CorLibTypeFactory.Byte);
        }

        /// <inheritdoc />
        public I4Value LoadElementU2(int index, ICliMarshaller marshaller)
        {
            return (I4Value) marshaller.ToCliValue(
                _contents.ReadInteger16(index * sizeof(ushort)),
                CorLibTypeFactory.UInt16);
        }

        /// <inheritdoc />
        public I4Value LoadElementU4(int index, ICliMarshaller marshaller)
        {
            return (I4Value) marshaller.ToCliValue(
                _contents.ReadInteger32(index * sizeof(uint)),
                CorLibTypeFactory.UInt32);
        }

        /// <inheritdoc />
        public FValue LoadElementR4(int index, ICliMarshaller marshaller)
        {
            return (FValue) marshaller.ToCliValue(
                _contents.ReadFloat32(index * sizeof(float)),
                CorLibTypeFactory.Single);
        }

        /// <inheritdoc />
        public FValue LoadElementR8(int index, ICliMarshaller marshaller)
        {
            return (FValue) marshaller.ToCliValue(
                _contents.ReadFloat64(index * sizeof(double)),
                CorLibTypeFactory.Double);
        }

        /// <inheritdoc />
        public OValue LoadElementRef(int index, ICliMarshaller marshaller)
        {
            return new OValue(null, false, marshaller.Is32Bit);
        }

        /// <inheritdoc />
        public void StoreElementI1(int index, I4Value value, ICliMarshaller marshaller)
        {
            _contents.WriteInteger8(index * sizeof(sbyte),
                (Integer8Value) marshaller.ToCtsValue(value, CorLibTypeFactory.SByte));
        }

        /// <inheritdoc />
        public void StoreElementI2(int index, I4Value value, ICliMarshaller marshaller)
        {
            _contents.WriteInteger16(index * sizeof(short),
                (Integer16Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Int16));
        }

        /// <inheritdoc />
        public void StoreElementI4(int index, I4Value value, ICliMarshaller marshaller)
        {
            _contents.WriteInteger32(index * sizeof(int),
                (Integer32Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Int32));
        }

        /// <inheritdoc />
        public void StoreElementI8(int index, I8Value value, ICliMarshaller marshaller)
        {
            _contents.WriteInteger64(index * sizeof(long),
                (Integer64Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Int64));
        }

        /// <inheritdoc />
        public void StoreElementU1(int index, I4Value value, ICliMarshaller marshaller)
        {
            _contents.WriteInteger8(index * sizeof(byte),
                (Integer8Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Byte));
        }

        /// <inheritdoc />
        public void StoreElementU2(int index, I4Value value, ICliMarshaller marshaller)
        {
            _contents.WriteInteger16(index * sizeof(ushort),
                (Integer16Value) marshaller.ToCtsValue(value, CorLibTypeFactory.UInt16));
        }

        /// <inheritdoc />
        public void StoreElementU4(int index, I4Value value, ICliMarshaller marshaller)
        {
            _contents.WriteInteger32(index * sizeof(uint),
                (Integer32Value) marshaller.ToCtsValue(value, CorLibTypeFactory.UInt32));
        }

        /// <inheritdoc />
        public void StoreElementR4(int index, FValue value, ICliMarshaller marshaller)
        {
            _contents.WriteFloat32(index * sizeof(float),
                (Float32Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Single));
        }

        /// <inheritdoc />
        public void StoreElementR8(int index, FValue value, ICliMarshaller marshaller)
        {
            _contents.WriteFloat64(index * sizeof(double),
                (Float64Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Double));
        }

        /// <inheritdoc />
        public void StoreElementRef(int index, OValue value, ICliMarshaller marshaller)
        {
            if (_contents.Is32Bit)
                _contents.WriteInteger32(index * sizeof(uint), new Integer32Value(0, 0));
            else
                _contents.WriteInteger64(index * sizeof(uint), new Integer64Value(0, 0));
        }


    }
}