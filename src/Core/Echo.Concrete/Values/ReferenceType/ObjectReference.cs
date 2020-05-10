using Echo.Core.Values;

namespace Echo.Concrete.Values.ReferenceType
{
    /// <summary>
    /// Represents a simple reference to an object.
    /// </summary>
    public class ObjectReference : IConcreteValue
    {
        /// <summary>
        /// Creates a new fully known reference to an object.
        /// </summary>
        /// <param name="referencedObject">The referenced object.</param>
        /// <param name="is32Bit">Indicates the pointer to the referenced object is 32 or 64 bits wide.</param>
        public ObjectReference(IConcreteValue referencedObject, bool is32Bit)
            : this(referencedObject, true, is32Bit)
        {
        }

        /// <summary>
        /// Creates a new reference to an object.
        /// </summary>
        /// <param name="referencedObject">The referenced object.</param>
        /// <param name="isKnown">Indicates the referenced object is known.</param>
        /// <param name="is32Bit">Indicates the pointer to the referenced object is 32 or 64 bits wide.</param>
        public ObjectReference(IConcreteValue referencedObject, bool isKnown, bool is32Bit)
        {
            Is32Bit = is32Bit;
            ReferencedObject = referencedObject;
            IsKnown = isKnown;
        }

        /// <summary>
        /// Gets the value of the object that is referenced.
        /// </summary>
        public IConcreteValue ReferencedObject
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the reference to the object is 32 or 64 bits wide.
        /// </summary>
        public bool Is32Bit
        {
            get;
        }

        /// <inheritdoc />
        public bool IsKnown
        {
            get;
        }

        /// <inheritdoc />
        public int Size => Is32Bit ? sizeof(uint) : sizeof(ulong);

        /// <inheritdoc />
        public bool IsValueType => false;

        /// <inheritdoc />
        public bool? IsZero => IsKnown ? ReferencedObject is null : (bool?) null;

        /// <inheritdoc />
        public bool? IsNonZero => !IsZero;

        /// <inheritdoc />
        public bool? IsPositive => !IsZero;

        /// <inheritdoc />
        public bool? IsNegative => false;

        /// <inheritdoc />
        public virtual IValue Copy() => new ObjectReference(ReferencedObject, IsKnown, Is32Bit);

        /// <inheritdoc />
        public override string ToString() => $"ObjectReference ({ReferencedObject})";
    }
}