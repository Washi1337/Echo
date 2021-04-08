using System;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Core;
using Echo.Core.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Represents a low level implementation of a structure.
    /// </summary>
    /// <remarks>
    /// This class is <strong>not</strong> meant to be used as an object reference. Instances of the
    /// <see cref="LleStructValue"/> class are passed on by-value. They are used for representing instances of value
    /// types, or the object referenced in an object reference, not the object reference itself. 
    /// </remarks>
    public partial class LleStructValue : IValueTypeValue
    {
        /// <summary>
        /// Creates a new low level emulated object. 
        /// </summary>
        /// <param name="valueFactory">The object responsible for memory management in the virtual machine.</param>
        /// <param name="valueType">The type of the object.</param>
        /// <param name="contents">The raw contents of the object.</param>
        public LleStructValue(IValueFactory valueFactory, TypeSignature valueType, IMemoryAccessValue contents)
        {
            Type = valueType ?? throw new ArgumentNullException(nameof(valueType));
            ValueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            Contents = contents ?? throw new ArgumentNullException(nameof(contents));
        }

        /// <summary>
        /// Gets the value factory that was used to create this structure.
        /// </summary>
        protected IValueFactory ValueFactory
        {
            get;
        }

        /// <summary>
        /// Indicates the value was constructed in a 32 or 64 bit environment.
        /// </summary>
        public bool Is32Bit => ValueFactory.Is32Bit;

        /// <inheritdoc />
        public TypeSignature Type
        {
            get;
        }

        /// <summary>
        /// The pointer to the raw data of the object.
        /// </summary>
        public IMemoryAccessValue Contents
        {
            get;
        }

        private CorLibTypeFactory CorLibTypeFactory => Type.Module.CorLibTypeFactory;

        /// <inheritdoc />
        public bool IsKnown => Contents.IsKnown;
        
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
        public IValue Copy() => new LleStructValue(ValueFactory, Type, Contents);
        
        /// <inheritdoc />
        public override string ToString() => $"{Type.FullName} ({Contents.Size.ToString()} bytes)";

        /// <inheritdoc />
        public void GetBits(Span<byte> buffer) => Contents.ReadBytes(0, buffer);

        /// <inheritdoc />
        public void GetMask(Span<byte> buffer)
        {
            Span<byte> data = stackalloc byte[buffer.Length];
            Contents.ReadBytes(0, data, buffer);
        }

        /// <inheritdoc />
        public void SetBits(ReadOnlySpan<byte> bits, ReadOnlySpan<byte> mask) => Contents.WriteBytes(0, bits, mask);
    }
}