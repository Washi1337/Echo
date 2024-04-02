using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Represents an address to an object (including its object header) within a CIL virtual machine. 
    /// </summary>
    public readonly partial struct ObjectHandle : IEquatable<ObjectHandle>
    {
        /// <summary>
        /// Creates a new object handle from the provided address.
        /// </summary>
        /// <param name="machine">The machine the address is valid in.</param>
        /// <param name="address">The address.</param>
        public ObjectHandle(CilVirtualMachine machine, long address)
        {
            Machine = machine;
            Address = address;
        }

        /// <summary>
        /// Gets the machine the object lives in.
        /// </summary>
        public CilVirtualMachine? Machine
        {
            get;
        }
        
        /// <summary>
        /// Gets the address to the beginning of the object.
        /// </summary>
        public long Address
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this handle represents the <c>null</c> reference.
        /// </summary>
        public bool IsNull => Address == 0;

        /// <summary>
        /// Gets the address to the beginning of the object's data.
        /// </summary>
        public StructHandle Contents
        {
            get
            {
                if (Machine is null)
                    return default;

                return new(Machine, Address + Machine.ValueFactory.TypeManager.ObjectHeaderSize);
            }
        }

        /// <summary>
        /// Gets the object's type (or method table).
        /// </summary>
        /// <returns>The type.</returns>
        [MemberNotNull(nameof(Machine))]
        public ITypeDescriptor GetObjectType()
        {
            AssertIsValidHandle();
            
            var pool = Machine.ValueFactory.BitVectorPool;
            var methodTableVector = pool.RentNativeInteger(Machine.Is32Bit, false);
            try
            {
                // Dereference the object pointer to get the bits for the method table pointer.
                var methodTableSpan = methodTableVector.AsSpan();
                Machine.Memory.Read(Address, methodTableSpan);

                if (!methodTableSpan.IsFullyKnown)
                    throw new ArgumentException("Object contains an unknown method table.");

                // Read the method table pointer.
                long methodTablePointer = methodTableSpan.ReadNativeInteger(Machine.Is32Bit);

                // Get corresponding method table (== type). 
                return Machine.ValueFactory.ClrMockMemory.MethodTables.GetObject(methodTablePointer);
            }
            finally
            {
                pool.Return(methodTableVector);
            }
        }

        [MemberNotNull(nameof(Machine))]
        private void AssertIsValidHandle()
        {
            if (Machine is null)
                throw new InvalidOperationException("Object handle is not associated to a machine.");
        }

        /// <summary>
        /// Obtains the object's data memory layout.
        /// </summary>
        /// <returns>The memory layout.</returns>
        public TypeMemoryLayout GetMemoryLayout()
        {
            AssertIsValidHandle();
            return Machine.ValueFactory.TypeManager.GetMethodTable(GetObjectType()).ContentsLayout;
        }

        /// <summary>
        /// Obtains the absolute address of a field within the object.
        /// </summary>
        /// <param name="field">The field to obtain the address for.</param>
        /// <returns>The address.</returns>
        public long GetFieldAddress(IFieldDescriptor field) => Contents.GetFieldAddress(field);

        /// <summary>
        /// Copies the value of a field into a new bit vector.
        /// </summary>
        /// <param name="field">The field to obtain the value for.</param>
        /// <returns>The value.</returns>
        public BitVector ReadField(IFieldDescriptor field) => Contents.ReadField(field);

        /// <summary>
        /// Copies the value of a field into a bit vector.
        /// </summary>
        /// <param name="field">The field to obtain the value for.</param>
        /// <param name="buffer">The buffer to copy the value into.</param>
        public void ReadField(IFieldDescriptor field, BitVectorSpan buffer) => Contents.ReadField(field, buffer);

        /// <summary>
        /// Copies the provided bit vector into a field of the object. 
        /// </summary>
        /// <param name="field">The field to write to.</param>
        /// <param name="buffer">The bits to write.</param>
        public void WriteField(IFieldDescriptor field, BitVectorSpan buffer) => Contents.WriteField(field, buffer);

        /// <summary>
        /// Interprets the handle as a string handle, and obtains the length of the contained string.
        /// </summary>
        /// <returns>The length.</returns>
        public BitVector ReadStringLength()
        {
            var buffer = new BitVector(32, false);
            ReadStringLength(buffer);
            return buffer;
        }

        /// <summary>
        /// Interprets the handle as a string handle, and obtains the length of the contained string.
        /// </summary>
        /// <param name="buffer">The buffer to copy the length bits into.</param>
        public void ReadStringLength(BitVectorSpan buffer)
        {
            AssertIsValidHandle();
            Machine.Memory.Read(Address + Machine.ValueFactory.TypeManager.StringLengthOffset, buffer);
        }

        /// <summary>
        /// Interprets the handle as a string handle, and obtains the bits that make up the characters of the string.
        /// </summary>
        /// <returns>The bit vector containing the string's characters.</returns>
        /// <exception cref="ArgumentException">Occurs when the string has an unknown length.</exception>
        public BitVector ReadStringData()
        {
            AssertIsValidHandle();
            var length = Machine.ValueFactory.BitVectorPool.Rent(32, false);
            try
            {
                ReadStringLength(length);
                if (!length.AsSpan().IsFullyKnown)
                    throw new ArgumentException("The string has an unknown length.");

                var result = new BitVector(length.AsSpan().I32 * sizeof(char) * 8, false);
                Machine.Memory.Read(Address + Machine.ValueFactory.TypeManager.StringHeaderSize, result);
                return result;
            }
            finally
            {
                Machine.ValueFactory.BitVectorPool.Return(length);
            }
        }

        /// <summary>
        /// Interprets the handle as an array reference, and obtains the length of the contained array.
        /// </summary>
        /// <returns>The bits representing the length of the array.</returns>
        public BitVector ReadArrayLength()
        {
            var buffer = new BitVector(32, false);
            ReadArrayLength(buffer);
            return buffer;
        }

        /// <summary>
        /// Interprets the handle as an array reference, and obtains the length of the contained array.
        /// </summary>
        /// <param name="buffer">The buffer to copy the length bits into.</param>
        public void ReadArrayLength(BitVectorSpan buffer)
        {
            AssertIsValidHandle();
            Machine.Memory.Read(Address + Machine.ValueFactory.TypeManager.ArrayLengthOffset, buffer);
        }

        /// <summary>
        /// Interprets the handle as an array reference, and obtains all elements of the array as one continuous buffer
        /// of bytes.
        /// </summary>
        /// <returns>The raw array data.</returns>
        /// <exception cref="ArgumentException">Occurs when the array has an unknown length.</exception>
        public BitVector ReadArrayData()
        {
            AssertIsValidHandle();
            var length = Machine.ValueFactory.BitVectorPool.Rent(32, false);
            try
            {
                // Read the length of the array.
                ReadArrayLength(length);
                if (!length.AsSpan().IsFullyKnown)
                    throw new ArgumentException("The array has an unknown length.");
                
                // Read the data.
                return ReadArrayData(0, length.AsSpan().I32);
            }
            finally
            {
                Machine.ValueFactory.BitVectorPool.Return(length);
            }   
        }
        
        /// <summary>
        /// Interprets the handle as an array reference, and obtains all elements of the array as one continuous buffer
        /// of bytes.
        /// </summary>
        /// <param name="startIndex">The index to start reading from.</param>
        /// <param name="length">The number of elements to read.</param>
        /// <returns>The raw array data.</returns>
        /// <exception cref="ArgumentException">Occurs when the array has an unknown length.</exception>
        public BitVector ReadArrayData(int startIndex, int length)
        {
            var arrayType = GetObjectType();
            if (arrayType is not SzArrayTypeSignature { BaseType: { } elementType })
                throw new ArgumentException("The object handle does not point to an array type");
            
            // Determine total space required to store the array data.
            int elementSize = (int) Machine.ValueFactory.TypeManager.GetMethodTable(elementType).ValueLayout.Size;
            var result = new BitVector(length * elementSize * 8, false);
            
            // Read the data.
            ReadArrayData(result, startIndex, elementType);

            return result;
        }

        /// <summary>
        /// Interprets the handle as an array reference, and obtains all elements of the array as one continuous buffer
        /// of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to copy the array data into.</param>
        /// <returns>The raw array data.</returns>
        public void ReadArrayData(BitVectorSpan buffer)
        {
            AssertIsValidHandle();
            Machine.Memory.Read(Address + Machine.ValueFactory.TypeManager.ArrayHeaderSize, buffer);
        }

        /// <summary>
        /// Interprets the handle as an array reference, and obtains all elements of the array as one continuous buffer
        /// of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to copy the array data into.</param>
        /// <param name="startIndex">The start index to read from.</param>
        /// <returns>The raw array data.</returns>
        public void ReadArrayData(BitVectorSpan buffer, int startIndex)
        {
            var arrayType = GetObjectType();
            if (arrayType is not SzArrayTypeSignature { BaseType: { } elementType })
                throw new ArgumentException("The object handle does not point to an array type");

            ReadArrayData(buffer, startIndex, elementType);
        }

        /// <summary>
        /// Interprets the handle as an array reference, and obtains all elements of the array as one continuous buffer
        /// of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to copy the array data into.</param>
        /// <param name="startIndex">The start index to read from.</param>
        /// <param name="elementType">The element type to assume.</param>
        /// <returns>The raw array data.</returns>
        public void ReadArrayData(BitVectorSpan buffer, int startIndex, TypeSignature elementType)
        {
            AssertIsValidHandle();
            int elementSize = (int) Machine.ValueFactory.TypeManager.GetMethodTable(elementType).ValueLayout.Size;
            Machine.Memory.Read(Address + Machine.ValueFactory.TypeManager.ArrayHeaderSize + startIndex * elementSize, buffer);
        }

        /// <summary>
        /// Interprets the handle as an array reference, and writes elements to the array's data as one continuous buffer
        /// of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to copy the array data from.</param>
        public void WriteArrayData(BitVectorSpan buffer)
        {
            AssertIsValidHandle();
            Machine.Memory.Write(Address + Machine.ValueFactory.TypeManager.ArrayHeaderSize, buffer);
        }

        /// <summary>
        /// Interprets the handle as an array reference, and writes elements to the array's data as one continuous buffer
        /// of bytes.
        /// </summary>
        /// <param name="buffer">The buffer to copy the array data from.</param>
        public void WriteArrayData(ReadOnlySpan<byte> buffer)
        {
            AssertIsValidHandle();
            Machine.Memory.Write(Address + Machine.ValueFactory.TypeManager.ArrayHeaderSize, buffer);
        }
        
        /// <summary>
        /// Interprets the handle as an array reference, and obtains the address of the provided element by its index.
        /// </summary>
        /// <param name="elementType">The type of elements the array stores.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns>The address of the element.</returns>
        public long GetArrayElementAddress(TypeSignature elementType, long index)
        {
            AssertIsValidHandle();
            return Address + Machine.ValueFactory.TypeManager.GetArrayElementOffset(elementType, index);
        }

        /// <summary>
        /// Interprets the handle as an array reference, and reads an element by its index.
        /// </summary>
        /// <param name="elementType">The type of elements the array stores.</param>
        /// <param name="index">The index of the element.</param>
        /// <returns>The bits of the element.</returns>
        public BitVector ReadArrayElement(TypeSignature elementType, long index)
        {
            AssertIsValidHandle();
            var buffer = Machine.ValueFactory.CreateValue(elementType, false);
            ReadArrayElement(elementType, index, buffer);
            return buffer;
        }

        /// <summary>
        /// Interprets the handle as an array reference, and reads an element by its index.
        /// </summary>
        /// <param name="elementType">The type of elements the array stores.</param>
        /// <param name="index">The index of the element.</param>
        /// <param name="buffer">The buffer to write the bits of the element into.</param>
        public void ReadArrayElement(TypeSignature elementType, long index, BitVectorSpan buffer)
        {
            AssertIsValidHandle();
            Machine.Memory.Read(GetArrayElementAddress(elementType, index), buffer);
        }

        /// <summary>
        /// Interprets the handle as an array reference, and writes a value to an element by its index.
        /// </summary>
        /// <param name="elementType">The type of elements the array stores.</param>
        /// <param name="index">The index of the element.</param>
        /// <param name="buffer">The bits of the element to write to the element.</param>
        public void WriteArrayElement(TypeSignature elementType, long index, BitVectorSpan buffer)
        {
            AssertIsValidHandle();
            Machine.Memory.Write(GetArrayElementAddress(elementType, index), buffer);
        }

        /// <summary>
        /// Reads the data stored in the object (excluding the object header), and stores it in a bit vector.
        /// </summary>
        /// <returns>The object's data.</returns>
        public BitVector ReadObjectData()
        {
            AssertIsValidHandle();
            var buffer = Machine.ValueFactory.CreateValue(GetObjectType().ToTypeSignature(), false);
            ReadObjectData(buffer);
            return buffer;
        }
        
        /// <summary>
        /// Reads the data stored in the object (excluding the object header), and stores it in a bit vector.
        /// </summary>
        /// <param name="type">The type to interpret the object as.</param>
        /// <returns>The object's data.</returns>
        public BitVector ReadObjectData(TypeSignature type)
        {
            AssertIsValidHandle();
            var buffer = Machine.ValueFactory.CreateValue(type, false);
            ReadObjectData(buffer);
            return buffer;
        }
        
        /// <summary>
        /// Reads the data stored in the object (excluding the object header), and stores it in a bit vector.
        /// </summary>
        /// <param name="buffer">The buffer to write the data to.</param>
        public void ReadObjectData(BitVectorSpan buffer)
        {
            AssertIsValidHandle();
            Machine.Memory.Read(Contents.Address, buffer);
        }

        /// <inheritdoc />
        public bool Equals(ObjectHandle other)
        {
            return Equals(Machine, other.Machine) && Address == other.Address;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is ObjectHandle other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Machine != null ? Machine.GetHashCode() : 0) * 397) ^ Address.GetHashCode();
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Machine is null
                ? "<invalid>"
                : $"0x{Address.ToString(Machine.Is32Bit ? "X8" : "X16")} ({Tag})";
        }
    }
}