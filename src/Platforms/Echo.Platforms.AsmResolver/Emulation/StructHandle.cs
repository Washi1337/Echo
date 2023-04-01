using System;
using AsmResolver.DotNet;
using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Represents an address to a structure within a CIL virtual machine. 
    /// </summary>
    public readonly struct StructHandle : IEquatable<StructHandle>
    {
        /// <summary>
        /// Creates a new struct handle from the provided address.
        /// </summary>
        /// <param name="machine">The machine the address is valid in.</param>
        /// <param name="address">The address.</param>
        public StructHandle(CilVirtualMachine machine, long address)
        {
            Machine = machine;
            Address = address;
        }

        /// <summary>
        /// Gets the machine the structures lives in.
        /// </summary>
        public CilVirtualMachine Machine
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
        /// Obtains the absolute address of a field within the structure.
        /// </summary>
        /// <param name="field">The field to obtain the address for.</param>
        /// <returns>The address.</returns>
        public long GetFieldAddress(IFieldDescriptor field)
        {
            return Address + Machine.ValueFactory.GetFieldMemoryLayout(field).Offset;
        }

        /// <summary>
        /// Copies the value of a field into a new bit vector.
        /// </summary>
        /// <param name="field">The field to obtain the value for.</param>
        /// <returns>The value.</returns>
        public BitVector ReadField(IFieldDescriptor field)
        {
            var buffer = Machine.ValueFactory.CreateValue(field.Signature!.FieldType, false);
            ReadField(field, buffer);
            return buffer;
        }

        /// <summary>
        /// Copies the value of a field into a bit vector.
        /// </summary>
        /// <param name="field">The field to obtain the value for.</param>
        /// <param name="buffer">The buffer to copy the value into.</param>
        public void ReadField(IFieldDescriptor field, BitVectorSpan buffer)
        {
            Machine.Memory.Read(GetFieldAddress(field), buffer);
        }

        /// <summary>
        /// Copies the provided bit vector into a field of the object. 
        /// </summary>
        /// <param name="field">The field to write to.</param>
        /// <param name="buffer">The bits to write.</param>
        public void WriteField(IFieldDescriptor field, BitVectorSpan buffer)
        {
            Machine.Memory.Write(GetFieldAddress(field), buffer);
        }

        /// <inheritdoc />
        public bool Equals(StructHandle other)
        {
            return Machine.Equals(other.Machine) && Address == other.Address;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is StructHandle other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Machine.GetHashCode() * 397) ^ Address.GetHashCode();
            }
        }
        
        /// <inheritdoc />
        public override string ToString() => Address.ToString(Machine.Is32Bit ? "X8" : "X16");
    }
}