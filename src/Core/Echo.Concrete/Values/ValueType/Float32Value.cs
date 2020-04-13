using System.Globalization;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a fully known concrete 32 bit floating point numerical value.
    /// </summary>
    public class Float32Value : IConcreteValue
    {
        /// <summary>
        /// Wraps a 32 bit floating point number into an instance of <see cref="Float32Value"/>.
        /// </summary>
        /// <param name="value">The 32 bit floating point number to wrap.</param>
        /// <returns>The concrete 32 bit floating point numerical value.</returns>
        public static implicit operator Float32Value(float value)
        {
            return new Float32Value(value);
        }

        /// <summary>
        /// Creates a new fully known concrete 32 bit floating point numerical value.
        /// </summary>
        /// <param name="value">The raw 32 bit value.</param>
        public Float32Value(float value)
        {
            F32 = value;
        }

        /// <summary>
        /// Gets or sets the raw floating point value.
        /// </summary>
        public float F32
        {
            get;
            set;
        }

        /// <inheritdoc />
        public bool IsKnown => true;

        /// <inheritdoc />
        public int Size => sizeof(float);

        /// <inheritdoc />
        public bool IsValueType => true;
        
        /// <inheritdoc />
        public IValue Copy() => new Float32Value(F32);

        /// <inheritdoc />
        public override string ToString() => F32.ToString(CultureInfo.InvariantCulture);

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            
            return obj is Float32Value value && F32.Equals(value.F32);;
        }

        /// <inheritdoc />
        public override int GetHashCode() => F32.GetHashCode();
    }
}