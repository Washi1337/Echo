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
        public int Size => 0;

        /// <inheritdoc />
        public bool IsValueType => false;

        /// <inheritdoc />
        public IValue Copy() => new UnknownValue();
    }
}