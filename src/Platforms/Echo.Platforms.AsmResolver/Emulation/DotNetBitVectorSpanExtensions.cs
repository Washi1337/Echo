using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides methods for manipulating bit vectors that represent managed objects. 
    /// </summary>
    public static class DotNetBitVectorExtensions
    {
        /// <summary>
        /// Interprets the span as an object pointer, and obtains the type of the object that it is referencing.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed object.</param>
        /// <param name="machine">The machine that this vector resides in.</param>
        /// <returns>The object type.</returns>
        public static ITypeDescriptor GetObjectPointerType(this BitVectorSpan span, CilVirtualMachine machine)
        {
            if (!span.IsFullyKnown)
                throw new ArgumentException("Cannot dereference a partially unknown pointer.");
            
            var pool = machine.ValueFactory.BitVectorPool;
            
            var methodTableVector = pool.RentNativeInteger(machine.Is32Bit, false);
            try
            {
                // Dereference the object pointer to get the bits for the method table pointer.
                var methodTableSpan = methodTableVector.AsSpan();
                machine.Memory.Read(span.ReadNativeInteger(machine.Is32Bit), methodTableSpan);

                // Read the method table pointer.
                long methodTablePointer = methodTableSpan.ReadNativeInteger(machine.Is32Bit);

                // Get corresponding method table (== type). 
                return machine.ValueFactory.ClrMockMemory.MethodTables.GetObject(methodTablePointer);
            }
            finally
            {
                pool.Return(methodTableVector);
            }
        }
        
        /// <summary>
        /// Interprets the bit vector as the contents of a managed object, and carves out the object's method table
        /// pointer.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed object.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the pointer to the object's method table.</returns>
        public static BitVectorSpan SliceObjectMethodTable(this BitVectorSpan span, ValueFactory factory)
        {
            return span.Slice(0, (int) factory.PointerSize * 8);
        }

        /// <summary>
        /// Interprets the bit vector as the contents of a managed object, and carves out the actual object data.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed object.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains just the object data.</returns>
        /// <remarks>
        /// This method effectively strips away the header of an object.
        /// </remarks>
        public static BitVectorSpan SliceObjectData(this BitVectorSpan span, ValueFactory factory)
        {
            return span.Slice((int) (factory.ObjectHeaderSize * 8));
        }

        /// <summary>
        /// Interprets the bit vector as object data (including the object header), and carves out a single field from it.
        /// </summary>
        /// <param name="span">The bit vector representing the entire object.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <param name="field">The field to carve out.</param>
        /// <returns>The slice that contains the raw data of the field.</returns>
        /// <exception cref="ArgumentException">
        /// Occurs when the provided field has no valid declaring type or could not be resolved.
        /// </exception>
        public static BitVectorSpan SliceObjectField(
            this BitVectorSpan span, 
            ValueFactory factory,
            IFieldDescriptor field)
        {
            return span.SliceObjectData(factory).SliceStructField(factory, field);
        }
        
        /// <summary>
        /// Interprets the bit vector as the contents of a managed array, and carves out the length field.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed array.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the length of the array.</returns>
        public static BitVectorSpan SliceArrayLength(this BitVectorSpan span, ValueFactory factory)
        {
            return span.Slice((int) (factory.ArrayLengthOffset * 8), (int) (factory.PointerSize * 8));
        }

        /// <summary>
        /// Interprets the bit vector as the contents of a managed array, and carves out the raw data of all elements.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed array.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the raw data of the elements of the array.</returns>
        public static BitVectorSpan SliceArrayData(this BitVectorSpan span, ValueFactory factory)
        {
            return span.Slice((int) (factory.ArrayHeaderSize * 8));
        }

        /// <summary>
        /// Interprets the bit vector as the contents of a managed array, and carves out a single element.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed array.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <param name="elementType">The type of elements stored in the array.</param>
        /// <param name="index">The index of the element to carve out.</param>
        /// <returns>The slice that contains the raw data of the requested element.</returns>
        public static BitVectorSpan SliceArrayElement(this BitVectorSpan span, ValueFactory factory, TypeSignature elementType, int index)
        {
            uint size = factory.GetTypeValueMemoryLayout(elementType).Size;
            return span.Slice((int) (factory.ArrayHeaderSize + size * index) * 8, (int) (size * 8));
        }

        /// <summary>
        /// Interprets the bit vector as a string object, and carves out the length field.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed string object.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the length of the string.</returns>
        public static BitVectorSpan SliceStringLength(this BitVectorSpan span, ValueFactory factory)
        {
            return span.Slice((int) (factory.StringLengthOffset * 8), sizeof(uint) * 8);
        }
        
        /// <summary>
        /// Interprets the bit vector as a string object, and carves out the characters of the string.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed string object.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the raw characters of the string.</returns>
        public static BitVectorSpan SliceStringData(this BitVectorSpan span, ValueFactory factory)
        {
            return span.Slice((int) (factory.StringHeaderSize * 8));
        }

        /// <summary>
        /// Interprets the bit vector as a structure, and carves out a single field from it.
        /// </summary>
        /// <param name="span">The bit vector representing the entire structure.</param>
        /// <param name="factory">The object responsible for managing type layouts.</param>
        /// <param name="field">The field to carve out.</param>
        /// <returns>The slice that contains the raw data of the field.</returns>
        /// <remarks>
        /// When applying this on structures, this function can be used as is. When applying this on objects, make sure
        /// the input bit vector does <strong>not</strong> contain the object header. This header can be stripped away
        /// by using e.g. <see cref="SliceObjectData"/> first.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// Occurs when the provided field has no valid declaring type or could not be resolved.
        /// </exception>
        public static BitVectorSpan SliceStructField(this BitVectorSpan span, ValueFactory factory, IFieldDescriptor field)
        {
            var fieldMemoryLayout = factory.GetFieldMemoryLayout(field);
            return span.Slice((int) (fieldMemoryLayout.Offset * 8), (int) (fieldMemoryLayout.ContentsLayout.Size * 8));
        }
    }
}