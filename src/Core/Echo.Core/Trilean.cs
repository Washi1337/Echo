using System;

namespace Echo.Core
{
    /// <summary>
    /// Represents a ternary boolean (true, false or unknown) value. 
    /// </summary>
    public readonly struct Trilean
    {
        /// <summary>
        /// Represents the true value.
        /// </summary>
        public static readonly Trilean True = new Trilean(TrileanValue.True);
        
        /// <summary>
        /// Represents the false value.
        /// </summary>
        public static readonly Trilean False = new Trilean(TrileanValue.False);
        
        /// <summary>
        /// Represents the unknown value.
        /// </summary>
        public static readonly Trilean Unknown = new Trilean(TrileanValue.Unknown);
        
        /// <summary>
        /// Creates a new trilean.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        public Trilean(bool value)
        {
            Value = value ? TrileanValue.True : TrileanValue.False;
        }
        
        /// <summary>
        /// Creates a new trilean.
        /// </summary>
        /// <param name="value">The trilean value.</param>
        public Trilean(TrileanValue value)
        {
            Value = value;
        }
        
        /// <summary>
        /// Gets the raw integer representation of the trilean value.
        /// </summary>
        public TrileanValue Value
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the value is known (either true or false).
        /// </summary>
        public bool IsKnown => Value != TrileanValue.Unknown;

        /// <summary>
        /// Gets a value indicating whether the value is unknown.
        /// </summary>
        public bool IsUnknown => Value == TrileanValue.Unknown;

        /// <summary>
        /// When the trilean value is known, obtains the boolean value.
        /// </summary>
        /// <returns>The boolean value.</returns>
        public bool ToBoolean() => Value switch
        {
            TrileanValue.Unknown => throw new ArgumentException("Trilean value is unknown."),
            TrileanValue.False => false,
            TrileanValue.True => true,
            _ => throw new ArgumentOutOfRangeException(nameof(Value))
        };

        /// <summary>
        /// When the trilean value is known, obtains the boolean value, otherwise returns <c>false</c>.
        /// </summary>
        /// <returns>The boolean value.</returns>
        public bool ToBooleanOrFalse() => Value == TrileanValue.True;
        
        /// <summary>
        /// Creates a new trilean.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        public static implicit operator Trilean(bool value) => new Trilean(value);
        
        /// <summary>
        /// Creates a new trilean.
        /// </summary>
        /// <param name="value">The trilean value.</param>
        public static implicit operator Trilean(TrileanValue value) => new Trilean(value);

        /// <summary>
        /// Determines whether the trilean is <c>true</c>.
        /// </summary>
        /// <param name="value">The trilean.</param>
        /// <returns><c>true</c> if the <see cref="Value"/> property is <see cref="TrileanValue.True"/>, <c>false</c> otherwise.</returns>
        public static bool operator true(Trilean value) => value.ToBooleanOrFalse();

        /// <summary>
        /// Determines whether the trilean is <c>false</c>.
        /// </summary>
        /// <param name="value">The trilean.</param>
        /// <returns><c>false</c> if the <see cref="Value"/> property is <see cref="TrileanValue.False"/>, <c>false</c> otherwise.</returns>
        public static bool operator false(Trilean value) => !value.ToBooleanOrFalse();

        /// <summary>
        /// Determines whether this trilean is exactly equal to the specified trilean.
        /// </summary>
        /// <param name="a">The left hand side of the comparison.</param>
        /// <param name="b">The right hand side of the comparison.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Value"/> property of both trileans are equal, <c>false</c> otherwise.
        /// </returns>
        public static bool operator ==(Trilean a, Trilean b) => a.Equals(b);

        /// <summary>
        /// Determines whether this trilean is not equal to the specified trilean.
        /// </summary>
        /// <param name="a">The left hand side of the comparison.</param>
        /// <param name="b">The right hand side of the comparison.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Value"/> property of both trileans are different, <c>false</c> otherwise.
        /// </returns>
        public static bool operator !=(Trilean a, Trilean b) => !a.Equals(b);

        /// <summary>
        /// Determines whether this trilean is exactly equal to the specified trilean.
        /// </summary>
        /// <param name="other">The other trilean.</param>
        /// <returns>
        /// <c>true</c> if the <see cref="Value"/> property of both trileans are equal, <c>false</c> otherwise.
        /// </returns>
        public bool Equals(Trilean other) => Value == other.Value;

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Trilean other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode() => (int) Value;
        
        /// <inheritdoc />
        public override string ToString() => Value switch
        {
            TrileanValue.Unknown => "?",
            TrileanValue.False => "0",
            TrileanValue.True => "1",
            _ => throw new ArgumentOutOfRangeException()
        };
        
    }
}