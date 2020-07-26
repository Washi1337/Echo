using System;
using System.Collections.Generic;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Core;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a low level implementation of an object.
    /// </summary>
    public partial class LleObjectValue : IValueTypeValue
    {
        private readonly IMemoryAllocator _memoryAllocator;

        /// <summary>
        /// Creates a new low level emulated object. 
        /// </summary>
        /// <param name="memoryAllocator">The object responsible for memory management in the virtual machine.</param>
        /// <param name="valueType">The type of the object.</param>
        /// <param name="contents">The raw contents of the object.</param>
        public LleObjectValue(IMemoryAllocator memoryAllocator, TypeSignature valueType, MemoryPointerValue contents)
        {
            Type = valueType;
            _memoryAllocator = memoryAllocator ?? throw new ArgumentNullException(nameof(memoryAllocator));
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public TypeSignature Type
        {
            get;
        }

        /// <summary>
        /// The pointer to the raw data of the object.
        /// </summary>
        public MemoryPointerValue Contents
        {
            get;
        }

        private CorLibTypeFactory CorLibTypeFactory => Type.Module.CorLibTypeFactory;

        /// <inheritdoc />
        public bool IsKnown => true;
        
        /// <inheritdoc />
        public bool Is32Bit => Contents.Is32Bit;
        
        /// <inheritdoc />
        public int Size => Contents.Size;

        /// <inheritdoc />
        public bool IsValueType => true;

        /// <inheritdoc />
        public Trilean IsZero => false;

        /// <inheritdoc />
        public Trilean IsNonZero => true;

        /// <inheritdoc />
        public Trilean IsPositive => true;

        /// <inheritdoc />
        public Trilean IsNegative => false;

        /// <inheritdoc />
        /// <inheritdoc />
        public IValue Copy() => new LleObjectValue(_memoryAllocator, Type, Contents);
        
        /// <inheritdoc />
        public override string ToString() => $"{Type.FullName} ({Contents.Length.ToString()} bytes)";

        /// <inheritdoc />
        public void GetBits(Span<byte> buffer) => Contents.ReadBytes(0, buffer);

        /// <inheritdoc />
        public void GetMask(Span<byte> buffer)
        {
            Span<byte> data = stackalloc byte[buffer.Length];
            Contents.ReadBytes(0, data, buffer);
        }

        /// <inheritdoc />
        public void SetBits(Span<byte> bits, Span<byte> mask) => Contents.WriteBytes(0, bits, mask);
    }
}