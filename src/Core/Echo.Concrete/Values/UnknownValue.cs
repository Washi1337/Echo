using Echo.Core;
using Echo.Core.Emulation;

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
        Trilean IConcreteValue.IsZero => Trilean.Unknown;

        /// <inheritdoc />
        Trilean IConcreteValue.IsNonZero => Trilean.Unknown;

        /// <inheritdoc />
        Trilean IConcreteValue.IsPositive => Trilean.Unknown;

        /// <inheritdoc />
        Trilean IConcreteValue.IsNegative => Trilean.Unknown;
        
        /// <inheritdoc />
        public IValue Copy() => new UnknownValue();

        /// <inheritdoc />
        public override string ToString() => "?";
    }
}