using Echo.Core.Values;

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
        /// <remarks>
        /// If this value is <c>null</c>, it is unknown whether this value is null or contains only zeroes.
        /// </remarks>
        bool? IsZero
        {
            get;
        }

        /// <summary>
        /// Determines whether the value is not null or contains at least a single one in its bit string.
        /// </summary>
        /// <remarks>
        /// If this value is <c>null</c>, it is unknown whether this value is not null or contains at least a single
        /// one in its bit string.
        /// </remarks>
        bool? IsNonZero
        {
            get;
        }

        /// <summary>
        /// Determines whether the value contains a positive value.
        /// </summary>
        /// <remarks>
        /// If this value is <c>null</c>, it is unknown whether this value is positive or not.
        /// </remarks>
        bool? IsPositive
        {
            get;
        }

        /// <summary>
        /// Determines whether the value contains a negative value.
        /// </summary>
        /// <remarks>
        /// If this value is <c>null</c>, it is unknown whether this value is negative or not.
        /// </remarks>
        bool? IsNegative
        {
            get;
        }
    }
}