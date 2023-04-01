using System;
using System.IO;
using System.Text;
using Echo.Concrete;
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
            
            return DeserializeObject(pointer, targetType);
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