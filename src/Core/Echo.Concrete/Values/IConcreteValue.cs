using Echo.Core;
using Echo.Core.Emulation;

namespace Echo.Concrete.Values
{
    /// <summary>
    /// Provides a base for all virtualized concrete values.
    /// </summary>
    public interface IConcreteValue : IValue
    {
        /// <summary>
        /// Gets a value indicating whether the object is passed on by value or by reference. 
        /// </summary>
        bool IsValueType
        {
            get;
        }

        /// <summary>
        /// Determines whether the value is null or consists of only zeroes.
        /// </summary>
        Trilean IsZero
        {
            get;
        }

        /// <summary>
        /// Determines whether the value is not null or contains at least a single one in its bit string.
        /// </summary>
        Trilean IsNonZero
        {
            get;
        }

        /// <summary>
        /// Determines whether the value contains a positive value.
        /// </summary>
        Trilean IsPositive
        {
            get;
        }

        /// <summary>
        /// Determines whether the value contains a negative value.
        /// </summary>
        Trilean IsNegative
        {
            get;
        }
    }
}