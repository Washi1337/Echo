using System;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values.ReferenceType;
using Echo.Core.Values;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a low level implementation of an object.
    /// </summary>
    public partial class LowLevelObjectValue 
    {
        private readonly IMemoryAllocator _memoryAllocator;

        /// <summary>
        /// The pointer to the raw data of the object.
        /// </summary>
        private readonly MemoryPointerValue _contents;

        /// <summary>
        /// Creates a new low level emulated object. 
        /// </summary>
        /// <param name="memoryAllocator">The object responsible for memory management in the virtual machine.</param>
        /// <param name="valueType">The type of the object.</param>
        /// <param name="contents">The raw contents of the array.</param>
        public LowLevelObjectValue(IMemoryAllocator memoryAllocator, TypeSignature valueType, MemoryPointerValue contents)
        {
            Type = valueType;
            _memoryAllocator = memoryAllocator ?? throw new ArgumentNullException(nameof(memoryAllocator));
            _contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <inheritdoc />
        public TypeSignature Type
        {
            get;
        }

        private CorLibTypeFactory CorLibTypeFactory => Type.Module.CorLibTypeFactory;

        /// <inheritdoc />
        public bool IsKnown => true;
        
        /// <inheritdoc />
        public bool Is32Bit => _contents.Is32Bit;
        
        /// <inheritdoc />
        public int Size => _contents.Size;

        /// <inheritdoc />
        public bool IsValueType => false;

        /// <inheritdoc />
        public bool? IsZero => false;

        /// <inheritdoc />
        public bool? IsNonZero => true;

        /// <inheritdoc />
        public bool? IsPositive => true;

        /// <inheritdoc />
        public bool? IsNegative => false;

        /// <inheritdoc />
        public IValue Copy() => new LowLevelObjectValue(_memoryAllocator, Type, _contents);
        
        /// <inheritdoc />
        public override string ToString() => $"{Type.FullName} ({_contents.Length.ToString()} bytes)";

        
    }
}