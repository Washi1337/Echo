using System;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values.ReferenceType;
using Echo.Core;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a low level implementation of an object.
    /// </summary>
    public partial class LleObjectValue 
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
        public bool IsValueType => Type.IsValueType;

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

        
    }
}