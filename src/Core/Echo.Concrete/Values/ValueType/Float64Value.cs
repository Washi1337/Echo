using System.Globalization;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ValueType
{
    /// <summary>
    /// Represents a fully known concrete 64 bit floating point numerical value.
    /// </summary>
    public class Float64Value : IConcreteValue
    {
        /// <summary>
        /// Wraps a 64 bit floating point number into an instance of <see cref="Float64Value"/>.
        /// </summary>
        /// <param name="value">The 64 bit floating point number to wrap.</param>
        /// <returns>The concrete 64 bit floating point numerical value.</returns>
        public static implicit operator Float64Value(double value)
        {
            return new Float64Value(value);
        }

        /// <summary>
        /// Creates a new fully known concrete 64 bit floating point numerical value.
        /// </summary>
        /// <param name="value">The raw 64 bit value.</param>
        public Float64Value(double value)
        {
            R8 = value;
        }

        /// <summary>
        /// Gets or sets the raw floating point value.
        /// </summary>
        public double R8
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
        public IValue Copy() => new Float64Value(R8);

        /// <inheritdoc />
        public override string ToString() => R8.ToString(CultureInfo.InvariantCulture);
    }
}