using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using Echo.Memory;

namespace Echo.Platforms.AsmResolver.Emulation.Heap
{
    /// <summary>
    /// Embeds a managed object into the memory of a virtual machine, providing HLE access to the object.
    /// </summary>
    public class MappedObject : IMemorySpace
    {
        private readonly Dictionary<uint, FieldInfo> _fields = new();
        private readonly CilVirtualMachine _machine;
        private readonly TypeMemoryLayout _virtualLayout;
        private long _baseAddress;

        /// <summary>
        /// Creates a new managed object embedding.
        /// </summary>
        /// <param name="o">The object to embed.</param>
        /// <param name="virtualLayout">The memory layout of the object.</param>
        /// <param name="machine">The machine the object is valid in.</param>
        public MappedObject(object o, TypeMemoryLayout virtualLayout, CilVirtualMachine machine)
        {
            Object = o ?? throw new ArgumentNullException(nameof(o));
            _virtualLayout = virtualLayout;
            _machine = machine;
        }

        /// <summary>
        /// Gets the embedded object.
        /// </summary>
        public object Object
        {
            get;
        }

        /// <summary>
        /// Gets the virtual memory layout of the object's contents.
        /// </summary>

        /// <inheritdoc />
        public AddressRange AddressRange => new(_baseAddress, _baseAddress + _machine.ValueFactory.ObjectHeaderSize + _virtualLayout.Size);

        /// <inheritdoc />
        public bool IsValidAddress(long address) => AddressRange.Contains(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _baseAddress = baseAddress;

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            buffer.MarkFullyUnknown();

            // First field in an object is always its method table (i.e., pointer to its type).
            uint offset = (uint) (address - _baseAddress);
            if (offset == 0)
            {
                long methodTable = _machine.ValueFactory.ClrMockMemory.MethodTables.GetAddress(_virtualLayout.Type);
                buffer.Write(methodTable);
                return;
            }
            
            if (Object.GetType().IsArray)
                ReadFromArray(offset, buffer);
            else
                ReadFromObject(offset, buffer);
        }

        private void ReadFromArray(uint offset, BitVectorSpan buffer)
        {
            var array = (Array) Object;
            
            if (offset == _machine.ValueFactory.ArrayLengthOffset)
            {
                buffer.Write(array.Length);
            }
            else if (offset >= _machine.ValueFactory.ArrayHeaderSize)
            {
                // Determine array element index.
                var representative = GetArrayElementRepresentative();
                uint size = _machine.ValueFactory.GetTypeValueMemoryLayout(representative).Size;
                int index = (int) ((offset - _machine.ValueFactory.ArrayHeaderSize) / size);
                
                // Read + Marshal
                object value = array.GetValue(index);
                buffer.Write(_machine.ObjectMarshaller.ToBitVector(value));
            }
        }

        private void ReadFromObject(uint offset, BitVectorSpan buffer)
        {
            // Find the field corresponding to the field offset.
            var field = GetFieldInfoAtOffset(offset);
            if (field is null)
                return;

            // Read + Marshal
            object? value = field.GetValue(Object);
            buffer.Write(_machine.ObjectMarshaller.ToBitVector(value));
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            uint offset = (uint) (address - _baseAddress);

            if (Object.GetType().IsArray)
                WriteToArray(offset, buffer);
            else
                WriteToObject(offset, buffer);
        }

        private void WriteToArray(uint offset, BitVectorSpan buffer)
        {
            if (offset < _machine.ValueFactory.ArrayHeaderSize)
                return;

            var array = (Array) Object;
            
            // Determine array element index.
            var representative = GetArrayElementRepresentative();
            uint size = _machine.ValueFactory.GetTypeValueMemoryLayout(representative).Size;
            int index = (int) ((offset - _machine.ValueFactory.ArrayHeaderSize) / size);

            // Marshal + Write.
            object? value = _machine.ObjectMarshaller.ToObject(buffer, Object.GetType().GetElementType()!);
            array.SetValue(value, index);
        }

        private void WriteToObject(uint offset, BitVectorSpan buffer)
        {
            // Find the field corresponding to the field offset.
            var field = GetFieldInfoAtOffset(offset);
            if (field is null)
                return;

            // Marshal + Write.
            object? value = _machine.ObjectMarshaller.ToObject(buffer, field.FieldType);
            field.SetValue(Object, value);
        }

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer)
        {
            var vector = new BitVector(buffer.Length * 8, false);
            vector.AsSpan().Write(buffer);
            Write(address, vector);
        }

        private ITypeDescriptor GetArrayElementRepresentative()
        {
            var elementType = Object.GetType().GetElementType()!;

            var factory = _machine.ContextModule.CorLibTypeFactory;
            var representative = Type.GetTypeCode(elementType) switch
            {
                TypeCode.Boolean => factory.Boolean,
                TypeCode.Byte => factory.Byte,
                TypeCode.Char => factory.Char,
                TypeCode.DateTime => factory.Int64,
                TypeCode.Decimal => _machine.ValueFactory.DecimalType,
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
            return representative;
        }

        private FieldInfo? GetFieldInfoAtOffset(uint offset)
        {
            // Skip object header.
            offset -= _machine.ValueFactory.ObjectHeaderSize;
            
            // See if we have looked up the field already before.
            if (_fields.TryGetValue(offset, out var fieldInfo))
                return fieldInfo;
            
            // Find target field.
            if (!_virtualLayout!.TryGetFieldAtOffset(offset, out var field))
                return null;

            // Find corresponding reflection field.
            fieldInfo = Object.GetType().GetRuntimeFields().First(f =>
                f.Name == field.Field.Name
                && field.Field.Signature!.FieldType.IsTypeOf(f.FieldType.Namespace, f.FieldType.Name));

            // Save in cache.
            _fields.Add(offset, fieldInfo);
            
            return fieldInfo;
        }

    }
}