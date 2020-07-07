using System;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    public partial class LowLevelObjectValue : IDotNetArrayValue
    {
        // -------------------------
        // Implementation rationale
        // -------------------------
        //
        // Arrays in .NET are implemented as a single memory region containing all elements of the array in sequence,
        // and are referenced by a single pointer to the first item of the array.
        //
        // The ldelem instruction is responsible for reading elements from this array, which is implemented like a normal
        // pointer access:
        // 
        //     T value = ((T*) array)[index];
        //
        // A side-effect of this is that it is possible to read "elements" from a different element type than the
        // original array type's element type. For example, it is possible (although undocumented) to read an int32
        // from an int8 array using the ldelem.i4 instruction, effectively reading 4 elements at once. Although
        // peverify does complain about this, current implementations of the runtime (tested on .NET Core and Mono)
        // does not do any type checking on this.
        //
        // Even worse, this opens up for reading outside of the bounds of the memory region. The runtime only checks
        // whether the index is smaller than the length of the array, and if it is not, it throws an
        // IndexOutOfRangeException. It does not, however, throw an exception when requesting a larger element for
        // which the offset technically falls outside of the memory range (for example, requesting the last element in
        // an int8 array as an int32). Interestingly enough, it does not segfault / throw an access violation exception,
        // but instead it always seems to return 0 (even for very large arrays).
        //
        // Writing to an invalid offset, however, does in fact throw an access violation exception.
        //
        // This implementation of array attempts to emulate this behaviour. This is also the reason we use a raw
        // memory pointer as well for storing the elements as opposed to a normal IConcreteValue[]. To make sure 
        // we are not corrupting our system or try to access memory outside of the allocated memory, we use the
        // method IsInRange next to AssertIndexValidity to make sure that even if the index is technically valid,
        // the memory address is also valid, and otherwise return 0.
        
        /// <inheritdoc />
        public int Length
        {
            get
            {
                var elementType = Type is SzArrayTypeSignature szArrayType
                    ? szArrayType.BaseType
                    : Type.Module.CorLibTypeFactory.Byte;

                var elementTypeLayout = _memoryAllocator.GetTypeMemoryLayout(elementType);
                return _contents.Length / (int) elementTypeLayout.Size;
            }
        }

        private void AssertIndexValidity(int index)
        {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();
        }

        private bool OffsetIsInRange(int index, int elementSize) => index * elementSize < _contents.Length;
        
        /// <inheritdoc />
        public NativeIntegerValue LoadElementI(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            
            IntegerValue rawValue;
            if (_contents.Is32Bit)
            {
                rawValue = OffsetIsInRange(index, sizeof(uint))
                    ? _contents.ReadInteger32(index * sizeof(uint))
                    : new Integer32Value(0);
            }
            else
            {
                rawValue = OffsetIsInRange(index, sizeof(ulong))
                    ? _contents.ReadInteger64(index * sizeof(ulong))
                    : new Integer64Value(0);
            }

            return (NativeIntegerValue) marshaller.ToCliValue(rawValue, CorLibTypeFactory.IntPtr);
        }

        /// <inheritdoc />
        public I4Value LoadElementI1(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            var rawValue = OffsetIsInRange(index, sizeof(sbyte))
                ? _contents.ReadInteger8(index * sizeof(sbyte))
                : new Integer8Value(0);
            
            return (I4Value) marshaller.ToCliValue(rawValue, CorLibTypeFactory.SByte);
        }

        /// <inheritdoc />
        public I4Value LoadElementI2(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            var rawValue = OffsetIsInRange(index, sizeof(short))
                ? _contents.ReadInteger16(index * sizeof(short))
                : new Integer16Value(0);
            
            return (I4Value) marshaller.ToCliValue(rawValue, CorLibTypeFactory.Int16);
        }

        /// <inheritdoc />
        public I4Value LoadElementI4(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            var rawValue = OffsetIsInRange(index, sizeof(int))
                ? _contents.ReadInteger32(index * sizeof(int))
                : new Integer32Value(0);
            
            return (I4Value) marshaller.ToCliValue(rawValue, CorLibTypeFactory.Int32);
        }

        /// <inheritdoc />
        public I8Value LoadElementI8(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            
            var rawValue = OffsetIsInRange(index, sizeof(long))
                ? _contents.ReadInteger64(index * sizeof(long))
                : new Integer64Value(0);
            
            return (I8Value) marshaller.ToCliValue(rawValue, CorLibTypeFactory.Int64);
        }

        /// <inheritdoc />
        public I4Value LoadElementU1(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            
            var rawValue = OffsetIsInRange(index, sizeof(byte))
                ? _contents.ReadInteger8(index * sizeof(byte))
                : new Integer8Value(0);
            
            return (I4Value) marshaller.ToCliValue(rawValue, CorLibTypeFactory.Byte);
        }

        /// <inheritdoc />
        public I4Value LoadElementU2(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            
            var rawValue = OffsetIsInRange(index, sizeof(ushort))
                ? _contents.ReadInteger16(index * sizeof(ushort))
                : new Integer16Value(0);
            
            return (I4Value) marshaller.ToCliValue(rawValue, CorLibTypeFactory.UInt16);
        }

        /// <inheritdoc />
        public I4Value LoadElementU4(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            
            var rawValue = OffsetIsInRange(index, sizeof(uint))
                ? _contents.ReadInteger32(index * sizeof(uint))
                : new Integer32Value(0);
            
            return (I4Value) marshaller.ToCliValue(rawValue, CorLibTypeFactory.UInt32);
        }

        /// <inheritdoc />
        public FValue LoadElementR4(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            
            var rawValue = OffsetIsInRange(index, sizeof(float))
                ? _contents.ReadFloat32(index * sizeof(float))
                : new Float32Value(0);
            
            return (FValue) marshaller.ToCliValue(rawValue, CorLibTypeFactory.Single);
        }

        /// <inheritdoc />
        public FValue LoadElementR8(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            
            var rawValue = OffsetIsInRange(index, sizeof(double))
                ? _contents.ReadFloat64(index * sizeof(double))
                : new Float64Value(0);
            
            return (FValue) marshaller.ToCliValue(rawValue, CorLibTypeFactory.Double);
        }

        /// <inheritdoc />
        public OValue LoadElementRef(int index, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            return new OValue(null, false, marshaller.Is32Bit);
        }

        /// <inheritdoc />
        public void StoreElementI(int index, NativeIntegerValue value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);
            
            var ctsValue = marshaller.ToCtsValue(value, CorLibTypeFactory.IntPtr);
            if (_contents.Is32Bit)
                _contents.WriteInteger32(index * sizeof(uint), (Integer32Value) ctsValue);
            else
                _contents.WriteInteger64(index * sizeof(uint), (Integer64Value) ctsValue);
        }

        /// <inheritdoc />
        public void StoreElementI1(int index, I4Value value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteInteger8(index * sizeof(sbyte),
                (Integer8Value) marshaller.ToCtsValue(value, CorLibTypeFactory.SByte));
        }

        /// <inheritdoc />
        public void StoreElementI2(int index, I4Value value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteInteger16(index * sizeof(short),
                (Integer16Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Int16));
        }

        /// <inheritdoc />
        public void StoreElementI4(int index, I4Value value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteInteger32(index * sizeof(int),
                (Integer32Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Int32));
        }

        /// <inheritdoc />
        public void StoreElementI8(int index, I8Value value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteInteger64(index * sizeof(long),
                (Integer64Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Int64));
        }

        /// <inheritdoc />
        public void StoreElementU1(int index, I4Value value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteInteger8(index * sizeof(byte),
                (Integer8Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Byte));
        }

        /// <inheritdoc />
        public void StoreElementU2(int index, I4Value value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteInteger16(index * sizeof(ushort),
                (Integer16Value) marshaller.ToCtsValue(value, CorLibTypeFactory.UInt16));
        }

        /// <inheritdoc />
        public void StoreElementU4(int index, I4Value value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteInteger32(index * sizeof(uint),
                (Integer32Value) marshaller.ToCtsValue(value, CorLibTypeFactory.UInt32));
        }

        /// <inheritdoc />
        public void StoreElementR4(int index, FValue value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteFloat32(index * sizeof(float),
                (Float32Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Single));
        }

        /// <inheritdoc />
        public void StoreElementR8(int index, FValue value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            _contents.WriteFloat64(index * sizeof(double),
                (Float64Value) marshaller.ToCtsValue(value, CorLibTypeFactory.Double));
        }

        /// <inheritdoc />
        public void StoreElementRef(int index, OValue value, ICliMarshaller marshaller)
        {
            AssertIndexValidity(index);

            if (_contents.Is32Bit)
                _contents.WriteInteger32(index * sizeof(uint), new Integer32Value(0, 0));
            else
                _contents.WriteInteger64(index * sizeof(uint), new Integer64Value(0, 0));
        }

    }
}