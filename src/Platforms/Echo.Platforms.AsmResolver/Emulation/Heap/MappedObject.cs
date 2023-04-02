using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Memory;
using Echo.Memory;
using Echo.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Heap
{
    /// <summary>
    /// Embeds a managed object into the memory of a virtual machine.
    /// </summary>
    public class MappedObject : IMemorySpace
    {
        private readonly Dictionary<uint, FieldInfo> _fields = new();
        private readonly CilVirtualMachine _machine;
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
            VirtualLayout = virtualLayout;
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
        public TypeMemoryLayout VirtualLayout
        {
            get;
        }

        /// <inheritdoc />
        public AddressRange AddressRange => new(
            _baseAddress, 
            _baseAddress + _machine.ValueFactory.ObjectHeaderSize + VirtualLayout.Size);

        /// <inheritdoc />
        public bool IsValidAddress(long address) => AddressRange.Contains(address);

        /// <inheritdoc />
        public void Rebase(long baseAddress) => _baseAddress = baseAddress;

        private FieldInfo? GetFieldInfoAtOffset(uint offset)
        {
            // Skip object header.
            offset -= _machine.ValueFactory.ObjectHeaderSize;
            
            // See if we have looked up the field already before.
            if (_fields.TryGetValue(offset, out var fieldInfo))
                return fieldInfo;
            
            // Find target field.
            if (!VirtualLayout.TryGetFieldAtOffset(offset, out var field))
                return null;

            // Find corresponding reflection field.
            fieldInfo = Object.GetType().GetRuntimeFields().First(f =>
                f.Name == field.Field.Name
                && field.Field.Signature!.FieldType.IsTypeOf(f.FieldType.Namespace, f.FieldType.Name));

            // Save in cache.
            _fields.Add(offset, fieldInfo);
            
            return fieldInfo;
        }

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            buffer.MarkFullyUnknown();
            
            uint offset = (uint) (address - _baseAddress);
            
            // First field in an object is always its method table (i.e., pointer to its type).
            if (offset == 0)
            {
                long methodTable = _machine.ValueFactory.ClrMockMemory.MethodTables.GetAddress(VirtualLayout.Type);
                buffer.Write(methodTable);
                return;
            }

            // Find the field.
            var field = GetFieldInfoAtOffset(offset);
            if (field is null)
                return;

            // Read.
            object? value = field.GetValue(Object);
            buffer.Write(_machine.ObjectMarshaller.ToBitVector(value));
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            uint offset = (uint) (address - _baseAddress);
            
            // Find the field.
            var field = GetFieldInfoAtOffset(offset);
            if (field is null)
                return;

            // Write.
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
    }
}