using System;
using AsmResolver.DotNet.Signatures.Types;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    public partial class LleObjectValue : IDotNetPointer
    {
        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer) => _contents.ReadBytes(offset, memoryBuffer);

        /// <inheritdoc />
        public void ReadBytes(int offset, Span<byte> memoryBuffer, Span<byte> knownBitmaskBuffer) => 
            _contents.ReadBytes(offset, memoryBuffer, knownBitmaskBuffer);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data) => _contents.WriteBytes(offset, data);

        /// <inheritdoc />
        public void WriteBytes(int offset, ReadOnlySpan<byte> data, ReadOnlySpan<byte> knownBitMask) => 
            _contents.WriteBytes(offset, data, knownBitMask);

        /// <inheritdoc />
        public Integer8Value ReadInteger8(int offset) => _contents.ReadInteger8(offset);

        /// <inheritdoc />
        public Integer16Value ReadInteger16(int offset) => _contents.ReadInteger16(offset);

        /// <inheritdoc />
        public Integer32Value ReadInteger32(int offset) => _contents.ReadInteger32(offset);

        /// <inheritdoc />
        public Integer64Value ReadInteger64(int offset) => _contents.ReadInteger64(offset);

        /// <inheritdoc />
        public Float32Value ReadFloat32(int offset) => _contents.ReadFloat32(offset);

        /// <inheritdoc />
        public Float64Value ReadFloat64(int offset) => _contents.ReadFloat64(offset);

        /// <inheritdoc />
        public IConcreteValue ReadStruct(int offset, TypeSignature type)
        {
            return type.ElementType switch
            {
                ElementType.Boolean => ReadInteger8(offset),
                ElementType.Char => ReadInteger16(offset),
                ElementType.I1 => ReadInteger8(offset),
                ElementType.U1 => ReadInteger8(offset),
                ElementType.I2 => ReadInteger16(offset),
                ElementType.U2 => ReadInteger16(offset),
                ElementType.I4 => ReadInteger32(offset),
                ElementType.U4 => ReadInteger32(offset),
                ElementType.I8 => ReadInteger64(offset),
                ElementType.U8 => ReadInteger64(offset),
                ElementType.R4 => ReadFloat32(offset),
                ElementType.R8 => ReadFloat64(offset),
                ElementType.ValueType => ReadStructSlow(offset, type),
                ElementType.I => Is32Bit ? (IntegerValue) ReadInteger32(offset) : ReadInteger64(offset),
                ElementType.U => Is32Bit ? (IntegerValue) ReadInteger32(offset) : ReadInteger64(offset),
                ElementType.Enum => ReadStruct(offset, type.Resolve().GetEnumUnderlyingType()),
                _ => new UnknownValue()
            };
        }

        private IConcreteValue ReadStructSlow(int offset, TypeSignature type)
        {
            var typeLayout = _memoryAllocator.GetTypeMemoryLayout(type);

            Span<byte> contents = stackalloc byte[(int) typeLayout.Size];
            Span<byte> bitmask = stackalloc byte[(int) typeLayout.Size];
            _contents.ReadBytes(offset, contents, bitmask);
            
            var structValue = _memoryAllocator.AllocateMemory((int) typeLayout.Size, false);
            structValue.WriteBytes(0, contents, bitmask);
            return structValue;
        }

        /// <inheritdoc />
        public void WriteInteger8(int offset, Integer8Value value) => _contents.WriteInteger8(offset, value);

        /// <inheritdoc />
        public void WriteInteger16(int offset, Integer16Value value) => _contents.WriteInteger16(offset, value);

        /// <inheritdoc />
        public void WriteInteger32(int offset, Integer32Value value) => _contents.WriteInteger32(offset, value);

        /// <inheritdoc />
        public void WriteInteger64(int offset, Integer64Value value) => _contents.WriteInteger64(offset, value);

        /// <inheritdoc />
        public void WriteFloat32(int offset, Float32Value value) => _contents.WriteFloat32(offset, value);

        /// <inheritdoc />
        public void WriteFloat64(int offset, Float64Value value) => _contents.WriteFloat64(offset, value);

        /// <inheritdoc />
        public void WriteStruct(int offset, TypeSignature type, IConcreteValue value)
        {
            switch (type.ElementType)
            {
                case ElementType.Boolean:
                case ElementType.I1:
                case ElementType.U1:
                    WriteInteger8(offset, (Integer8Value) value);
                    break;
                
                case ElementType.Char:
                case ElementType.I2:
                case ElementType.U2:
                    WriteInteger16(offset, (Integer16Value) value);
                    break;
                
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I when Is32Bit:
                case ElementType.U when Is32Bit:
                    WriteInteger32(offset, (Integer32Value) value);
                    break;
                
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.I when !Is32Bit:
                case ElementType.U when !Is32Bit:
                    WriteInteger64(offset, (Integer64Value) value);
                    break;
                
                case ElementType.R4:
                    WriteFloat32(offset, (Float32Value) value);
                    break;
                
                case ElementType.R8:
                    WriteFloat64(offset, (Float64Value) value);
                    break;
                
                case ElementType.ValueType:
                    WriteStructSlow(offset, type, value);
                    break;
                
                case ElementType.Enum:
                    WriteStruct(offset, type.Resolve().GetEnumUnderlyingType(), value);
                    break;
                
                case ElementType.String:
                case ElementType.Ptr:
                case ElementType.ByRef:
                case ElementType.Class:
                case ElementType.Array:
                case ElementType.GenericInst:
                case ElementType.FnPtr:
                case ElementType.Object:
                case ElementType.SzArray:
                    // We cannot really know the raw value of object pointers, write an unknown value.
                    if (Is32Bit)
                        WriteInteger32(offset, new Integer32Value(0, 0));
                    else 
                        WriteInteger64(offset, new Integer64Value(0, 0));
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void WriteStructSlow(int offset, TypeSignature type, IConcreteValue value)
        {
            var typeLayout = _memoryAllocator.GetTypeMemoryLayout(type);

            Span<byte> contents = stackalloc byte[(int) typeLayout.Size];
            Span<byte> bitmask = stackalloc byte[(int) typeLayout.Size];
            
            if (value is IValueTypeValue valueTypeValue)
            {
                // Value is a structure, we can get the raw contents of the struct.
                valueTypeValue.GetBits(contents);
                valueTypeValue.GetMask(bitmask);
            }
            else
            {
                // Value is not a struct value, mark bits as unknown.
                contents.Fill(0);
                bitmask.Fill(0);
            }
            
            WriteBytes(offset, contents, bitmask);
        }
        
    }
}