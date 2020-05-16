using Echo.Core.Values;

namespace Echo.Concrete.Values
{
    /// <summary>
    /// Represents the fully unknown value.
    /// </summary>
    public class UnknownValue : IConcreteValue
    {
        /// <inheritdoc />
        public bool IsKnown => false;

        /// <inheritdoc />
        int IValue.Size => 0;

        /// <inheritdoc />
        bool IConcreteValue.IsValueType => false;

        /// <inheritdoc />
        bool? IConcreteValue.IsZero => null;

        /// <inheritdoc />
        bool? IConcreteValue.IsNonZero => null;

        /// <inheritdoc />
        bool? IConcreteValue.IsPositive => null;

        /// <inheritdoc />
        bool? IConcreteValue.IsNegative => null;
        
        /// <inheritdoc />
        public IValue Copy() => new UnknownValue();

        /// <inheritdoc />
        public override string ToString() => "?";
    }
}