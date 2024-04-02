using System;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Runtime;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides methods for manipulating bit vectors that represent managed objects. 
    /// </summary>
    public static class DotNetBitVectorExtensions
    {
        /// <summary>
        /// Interprets a bit vector as a reference to an object.
        /// </summary>
        /// <param name="objectPointer">The bit vector containing the reference.</param>
        /// <param name="machine">The machine the address is valid in.</param>
        /// <returns>The object handle.</returns>
        /// <exception cref="ArgumentException">Occurs when the bit vector does not contain a fully known address.</exception>
        public static ObjectHandle AsObjectHandle(this BitVector objectPointer, CilVirtualMachine machine)
        {
            return objectPointer.AsSpan().AsObjectHandle(machine);
        }
        
        /// <summary>
        /// Interprets a bit vector as a reference to an object.
        /// </summary>
        /// <param name="objectPointer">The bit vector containing the reference.</param>
        /// <param name="machine">The machine the address is valid in.</param>
        /// <returns>The object handle.</returns>
        /// <exception cref="ArgumentException">Occurs when the bit vector does not contain a fully known address.</exception>
        public static ObjectHandle AsObjectHandle(this BitVectorSpan objectPointer, CilVirtualMachine machine)
        {
            if (!objectPointer.IsFullyKnown)
                throw new ArgumentException("Cannot create an object handle from a partially unknown bit vector");
            return new ObjectHandle(machine, objectPointer.ReadNativeInteger(machine.Is32Bit));
        }
        
        /// <summary>
        /// Interprets an integer as a reference to an object.
        /// </summary>
        /// <param name="objectPointer">The integer containing the reference.</param>
        /// <param name="machine">The machine the address is valid in.</param>
        /// <returns>The object handle.</returns>
        public static ObjectHandle AsObjectHandle(this long objectPointer, CilVirtualMachine machine)
        {
            return new ObjectHandle(machine, objectPointer);
        }
        
        /// <summary>
        /// Interprets a bit vector as a reference to a structure.
        /// </summary>
        /// <param name="objectPointer">The integer containing the reference.</param>
        /// <param name="machine">The machine the address is valid in.</param>
        /// <returns>The structure handle.</returns>
        /// <exception cref="ArgumentException">Occurs when the bit vector does not contain a fully known address.</exception>
        public static StructHandle AsStructHandle(this BitVector objectPointer, CilVirtualMachine machine)
        {
            return objectPointer.AsSpan().AsStructHandle(machine);
        }
        
        /// <summary>
        /// Interprets a bit vector as a reference to a structure.
        /// </summary>
        /// <param name="objectPointer">The integer containing the reference.</param>
        /// <param name="machine">The machine the address is valid in.</param>
        /// <returns>The structure handle.</returns>
        /// <exception cref="ArgumentException">Occurs when the bit vector does not contain a fully known address.</exception>
        public static StructHandle AsStructHandle(this BitVectorSpan objectPointer, CilVirtualMachine machine)
        {
            if (!objectPointer.IsFullyKnown)
                throw new ArgumentException("Cannot create an object handle from a partially unknown bit vector");
            return new StructHandle(machine, objectPointer.ReadNativeInteger(machine.Is32Bit));
        }
        
        /// <summary>
        /// Interprets an integer as a reference to a structure.
        /// </summary>
        /// <param name="objectPointer">The integer containing the reference.</param>
        /// <param name="machine">The machine the address is valid in.</param>
        /// <returns>The structure handle.</returns>=
        public static StructHandle AsStructHandle(this long objectPointer, CilVirtualMachine machine)
        {
            return new StructHandle(machine, objectPointer);
        }

        /// <summary>
        /// Interprets the bit vector as the contents of a managed object, and carves out the object's method table
        /// pointer.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed object.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the pointer to the object's method table.</returns>
        public static BitVectorSpan SliceObjectMethodTable(this BitVectorSpan span, RuntimeTypeManager manager)
        {
            return span.Slice(0, (int) manager.PointerSize * 8);
        }

        /// <summary>
        /// Interprets the bit vector as the contents of a managed object, and carves out the actual object data.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed object.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains just the object data.</returns>
        /// <remarks>
        /// This method effectively strips away the header of an object.
        /// </remarks>
        public static BitVectorSpan SliceObjectData(this BitVectorSpan span, RuntimeTypeManager manager)
        {
            return span.Slice((int) (manager.ObjectHeaderSize * 8));
        }

        /// <summary>
        /// Interprets the bit vector as object data (including the object header), and carves out a single field from it.
        /// </summary>
        /// <param name="span">The bit vector representing the entire object.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
        /// <param name="field">The field to carve out.</param>
        /// <returns>The slice that contains the raw data of the field.</returns>
        /// <exception cref="ArgumentException">
        /// Occurs when the provided field has no valid declaring type or could not be resolved.
        /// </exception>
        public static BitVectorSpan SliceObjectField(
            this BitVectorSpan span, 
            RuntimeTypeManager manager,
            IFieldDescriptor field)
        {
            return span.SliceObjectData(manager).SliceStructField(manager, field);
        }
        
        /// <summary>
        /// Interprets the bit vector as the contents of a managed array, and carves out the length field.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed array.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the length of the array.</returns>
        public static BitVectorSpan SliceArrayLength(this BitVectorSpan span, RuntimeTypeManager manager)
        {
            return span.Slice((int) (manager.ArrayLengthOffset * 8), (int) (manager.PointerSize * 8));
        }

        /// <summary>
        /// Interprets the bit vector as the contents of a managed array, and carves out the raw data of all elements.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed array.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the raw data of the elements of the array.</returns>
        public static BitVectorSpan SliceArrayData(this BitVectorSpan span, RuntimeTypeManager manager)
        {
            return span.Slice((int) (manager.ArrayHeaderSize * 8));
        }

        /// <summary>
        /// Interprets the bit vector as the contents of a managed array, and carves out a single element.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed array.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
        /// <param name="elementType">The type of elements stored in the array.</param>
        /// <param name="index">The index of the element to carve out.</param>
        /// <returns>The slice that contains the raw data of the requested element.</returns>
        public static BitVectorSpan SliceArrayElement(this BitVectorSpan span, RuntimeTypeManager manager, TypeSignature elementType, int index)
        {
            uint size = manager.GetMethodTable(elementType).ValueLayout.Size;
            return span.Slice((int) (manager.ArrayHeaderSize + size * index) * 8, (int) (size * 8));
        }

        /// <summary>
        /// Interprets the bit vector as a string object, and carves out the length field.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed string object.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the length of the string.</returns>
        public static BitVectorSpan SliceStringLength(this BitVectorSpan span, RuntimeTypeManager manager)
        {
            return span.Slice((int) (manager.StringLengthOffset * 8), sizeof(uint) * 8);
        }
        
        /// <summary>
        /// Interprets the bit vector as a string object, and carves out the characters of the string.
        /// </summary>
        /// <param name="span">The bit vector representing the entire managed string object.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
        /// <returns>The slice that contains the raw characters of the string.</returns>
        public static BitVectorSpan SliceStringData(this BitVectorSpan span, RuntimeTypeManager manager)
        {
            return span.Slice(
                (int)(manager.StringHeaderSize * 8),
                (int)((span.ByteCount - 2 - manager.StringHeaderSize) * 8)
            );
        }

        /// <summary>
        /// Interprets the bit vector as a structure, and carves out a single field from it.
        /// </summary>
        /// <param name="span">The bit vector representing the entire structure.</param>
        /// <param name="manager">The object responsible for managing type layouts.</param>
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
        public static BitVectorSpan SliceStructField(this BitVectorSpan span, RuntimeTypeManager manager, IFieldDescriptor field)
        {
            var fieldMemoryLayout = manager.GetFieldMemoryLayout(field);
            return span.Slice((int) (fieldMemoryLayout.Offset * 8), (int) (fieldMemoryLayout.ContentsLayout.Size * 8));
        }
    }
}