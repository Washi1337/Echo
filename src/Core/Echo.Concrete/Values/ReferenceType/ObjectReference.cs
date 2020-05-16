using Echo.Core.Values;

namespace Echo.Concrete.Values.ReferenceType
{
    /// <summary>
    /// Represents a simple reference to an object.
    /// </summary>
    public class ObjectReference : IConcreteValue
    {
        /// <summary>
        /// Creates a new null object reference value. 
        /// </summary>
        /// <param name="is32Bit">Indicates whether the reference to the object is 32 or 64 bits wide.</param>
        /// <returns>The null reference.</returns>
        public static ObjectReference Null(bool is32Bit) => new ObjectReference(null, true, is32Bit);
        
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

        /// <summary>
        /// Determines whether the object reference is equal to the provided object.
        /// </summary>
        /// <param name="other">The other object.</param>
        /// <returns><c>true</c> if the object are equal, <c>false</c> if not, and
        /// <c>null</c> if the conclusion of the comparison is not certain.</returns>
        public bool? IsEqualTo(ObjectReference other)
        {
            return IsKnown && IsKnown 
                ? (bool?) ReferenceEquals(ReferencedObject, other.ReferencedObject) 
                : null;
        }
        
        /// <summary>
        /// Determines whether the current object reference is considered greater than the provided object reference.
        /// </summary>
        /// <param name="other">The other object reference.</param>
        /// <returns><c>true</c> if the current value is greater than the provided value, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This method is only really reliable when one of the values is the null value. 
        /// </remarks>
        public bool? IsGreaterThan(ObjectReference other)
        {
            return IsZero switch
            {
                false when other is { IsZero: true } => true,
                true when other is { IsZero: false } => false,
                _ => null
            };
        }

        /// <summary>
        /// Determines whether the current object reference is considered less than the provided object reference.
        /// </summary>
        /// <param name="other">The other object reference.</param>
        /// <returns><c>true</c> if the current value is less than the provided value, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This method is only really reliable when one of the values is the null value. 
        /// </remarks>
        public bool? IsLessThan(ObjectReference other)
        {
            return IsZero switch
            {
                false when other is { IsZero: true } => false,
                true when other is { IsZero: false } => true,
                _ => null
            };
        }

        /// <summary>
        /// Determines whether the provided object references are considered equal. 
        /// </summary>
        /// <param name="other">The other object reference.</param>
        /// <returns><c>true</c> if the object references are equal, <c>false</c> otherwise.</returns>
        /// <remarks>
        /// This method verifies whether the actual contents of this object reference is equal to other. 
        /// This includes the case where both values are unknown, it returns <c>true</c>.
        /// This method should not be used within an emulation context to test whether two virtual object references
        /// are equal during the execution of a virtual machine.
        /// </remarks>
        protected bool Equals(ObjectReference other)
        {
            return Equals(ReferencedObject, other.ReferencedObject) && IsKnown == other.IsKnown;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != GetType())
                return false;
            return Equals((ObjectReference) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((ReferencedObject != null ? ReferencedObject.GetHashCode() : 0) * 397) ^ IsKnown.GetHashCode();
            }
        }
        
    }
}