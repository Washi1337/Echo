using System;
using System.IO;
using System.Text;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Heap;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides an implementation of the <see cref="IObjectMarshaller"/>, that embeds managed objects into the
    /// <see cref="ObjectMapMemory"/> of the virtual machine. 
    /// </summary>
    public class ObjectMarshaller : IObjectMarshaller
    {
        /// <summary>
        /// Creates a new marshaller for the provided virtual machine.
        /// </summary>
        /// <param name="machine">The machine.</param>
        public ObjectMarshaller(CilVirtualMachine machine)
        {
            Machine = machine;
        }

        /// <summary>
        /// Gets the machine the marshaller is targeting.
        /// </summary>
        public CilVirtualMachine Machine
        {
            get;
        }

        /// <inheritdoc />
        public virtual BitVector ToBitVector(object? obj)
        {
            if (obj is null)
                return Machine.ValueFactory.CreateNativeInteger(true);

            return Type.GetTypeCode(obj.GetType()) switch
            {
                TypeCode.Boolean => new BitVector((bool) obj ? (byte) 1 : (byte) 0),
                TypeCode.Byte => new BitVector((byte) obj),
                TypeCode.Char => new BitVector((char) obj),
                TypeCode.DateTime => CreateDateTimeVector((DateTime) obj),
                TypeCode.Decimal => CreateDecimalVector((decimal) obj),
                TypeCode.Double => new BitVector((double) obj),
                TypeCode.Int16 => new BitVector((short) obj),
                TypeCode.Int32 => new BitVector((int) obj),
                TypeCode.Int64 => new BitVector((long) obj),
                TypeCode.SByte => new BitVector((sbyte) obj),
                TypeCode.Single => new BitVector((float) obj),
                TypeCode.String => CreateStringReferenceVector((string) obj),
                TypeCode.UInt16 => new BitVector((ushort) obj),
                TypeCode.UInt32 => new BitVector((uint) obj),
                TypeCode.UInt64 => new BitVector((ulong) obj),
                _ => CreateObjectReferenceVector(obj)
            };
        }

        private static BitVector CreateDateTimeVector(DateTime dateTime)
        {
            return new BitVector(dateTime.Ticks);
        }

        private static BitVector CreateDecimalVector(decimal value)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(value);

            return new BitVector(stream.ToArray());
        }

        private BitVector CreateStringReferenceVector(string value)
        {
            long pointer = Machine.Heap.GetInternedString(value);
            return Machine.ValueFactory.CreateNativeInteger(pointer);
        }

        private BitVector CreateObjectReferenceVector(object obj)
        {
            var map = Machine.ObjectMapMemory.GetOrCreateMapping(obj);
            return Machine.ValueFactory.CreateNativeInteger(map.AddressRange.Start);
        }

        /// <inheritdoc />
        public virtual object? ToObject(BitVectorSpan vector, Type targetType)
        {
            if (!vector.IsFullyKnown)
                throw new ArgumentException($"Attempted to deserialize a vector to {targetType} with unknown bits.");
            
            return Type.GetTypeCode(targetType) switch
            {
                TypeCode.Boolean => !vector.IsZero.ToBoolean(),
                TypeCode.Byte => vector.U8,
                TypeCode.Char => (char) vector.U16,
                TypeCode.DateTime => ReadDateTimeVector(vector),
                TypeCode.Decimal => ReadDecimalVector(vector),
                TypeCode.Double => vector.F64,
                TypeCode.Int16 => vector.I16,
                TypeCode.Int32 => vector.I32,
                TypeCode.Int64 => vector.I64,
                TypeCode.SByte => vector.I8,
                TypeCode.Single => vector.F32,
                TypeCode.String => ReadStringReferenceVector(vector),
                TypeCode.UInt16 => vector.U16,
                TypeCode.UInt32 => vector.U32,
                TypeCode.UInt64 => vector.U64,
                _ when targetType.IsValueType => DeserializeStructure(vector, targetType),
                _ => ReadObjectReferenceVector(vector, targetType)
            };
        }

        private static DateTime ReadDateTimeVector(BitVectorSpan vector)
        {
            return new DateTime(vector.I64);
        }

        private static decimal ReadDecimalVector(BitVectorSpan vector)
        {
            using var stream = new MemoryStream(vector.Bits.ToArray());
            using var reader = new BinaryReader(stream);
            
            return reader.ReadDecimal();
        }

        private string? ReadStringReferenceVector(BitVectorSpan vector)
        {
            var data = vector.AsObjectHandle(Machine).ReadStringData();
            if (!data.AsSpan().IsFullyKnown)
                throw new ArgumentException("String contains one or more unknown characters.");

            return Encoding.Unicode.GetString(data.Bits);
        }

        private object? ReadObjectReferenceVector(BitVectorSpan vector, Type targetType)
        {
            long pointer = vector.ReadNativeInteger(Machine.Is32Bit);
            if (pointer == 0)
                return null;

            if (Machine.ObjectMapMemory.TryGetObject(pointer, out var map))
                return map.Object;

            if (targetType.IsArray && targetType.GetArrayRank() == 1)
                return DeserializeArray(pointer, targetType.GetElementType()!);
            
            return DeserializeObject(pointer, targetType);
        }

        private object? DeserializeArray(long pointer, Type elementType)
        {
            var factory = Machine.ContextModule.CorLibTypeFactory;
            var representative = Type.GetTypeCode(elementType) switch
            {
                TypeCode.Boolean => factory.Boolean,
                TypeCode.Byte => factory.Byte,
                TypeCode.Char => factory.Char,
                TypeCode.DateTime => factory.Int64,
                TypeCode.Decimal => Machine.ValueFactory.DecimalType.ToTypeSignature(),
                TypeCode.Double => factory.Double,
                TypeCode.Int16 => factory.Int16,
                TypeCode.Int32 => factory.Int32,
                TypeCode.Int64 => factory.Int64,
                TypeCode.SByte => factory.SByte,
                TypeCode.Single => factory.Single,
                TypeCode.UInt16 => factory.UInt16,
                TypeCode.UInt32 => factory.UInt32,
                TypeCode.UInt64 => factory.UInt64,
                _ when !elementType.IsValueType => factory.Object,
                _ => throw new NotSupportedException($"Could not deserialize an array with element type {elementType}.")
            };
            
            var arrayHandle = pointer.AsObjectHandle(Machine);
            var length = arrayHandle.ReadArrayLength().AsSpan();
            if (!length.IsFullyKnown)
                throw new ArgumentException("Array has an unknown length.");

            int lengthI32 = length.I32;
            var array = Array.CreateInstance(elementType, lengthI32);
            
            var buffer = Machine.ValueFactory.CreateValue(representative, false);
            for (int i = 0; i < lengthI32; i++)
            {
                arrayHandle.ReadArrayElement(representative, i, buffer);
                array.SetValue(ToObject(buffer, elementType), i);
            }

            return array;
        }

        /// <summary>
        /// Deserialize the provided bit vector to a structure of the provided type.
        /// </summary>
        /// <param name="data">The raw data of the structure</param>
        /// <param name="targetType">The type of object to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        protected virtual object DeserializeStructure(BitVectorSpan data, Type targetType)
        {
            throw new NotSupportedException($"Could not deserialize structure of type {targetType}.");
        }

        /// <summary>
        /// Deserialize the provided object address to an object of the provided type.
        /// </summary>
        /// <param name="pointer">The pointer to the beginning of the object.</param>
        /// <param name="targetType">The type of object to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        protected virtual object DeserializeObject(long pointer, Type targetType)
        {
            throw new NotSupportedException($"Could not deserialize object of type {targetType} at address 0x{pointer:X8}.");
        }
    }
}