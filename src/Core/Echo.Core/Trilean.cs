using System;
using System.Runtime.CompilerServices;

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
        
        // The following implements the following truth table:
        //
        //    | 0 | 1 | ?
        // ---+---+---+---
        //  0 | 0 | 0 | 0
        //  --+---+---+---
        //  1 | 0 | 1 | ?
        //  --+---+---+---
        //  ? | 0 | ? | ?
        private static readonly Trilean[] AndTable =
        {
            TrileanValue.False, TrileanValue.False, TrileanValue.False,
            TrileanValue.False, TrileanValue.True, TrileanValue.Unknown,
            TrileanValue.False, TrileanValue.Unknown, TrileanValue.Unknown,
        };
        
        // The following implements the following truth table:
        //
        //    | 0 | 1 | ?
        // ---+---+---+---
        //  0 | 0 | 1 | ?
        //  --+---+---+---
        //  1 | 1 | 1 | 1
        //  --+---+---+---
        //  ? | ? | 1 | ?
        private static readonly Trilean[] OrTable =
        {
            TrileanValue.False, TrileanValue.True, TrileanValue.Unknown,
            TrileanValue.True, TrileanValue.True, TrileanValue.True,
            TrileanValue.Unknown, TrileanValue.True, TrileanValue.Unknown,
        };
        
        // The following implements the following truth table:
        //
        //    | 0 | 1 | ?
        // ---+---+---+---
        //  0 | 0 | 1 | ?
        //  --+---+---+---
        //  1 | 1 | 0 | ?
        //  --+---+---+---
        //  ? | ? | ? | ?
        private static readonly Trilean[] XorTable =
        {
            TrileanValue.False, TrileanValue.True, TrileanValue.Unknown,
            TrileanValue.True, TrileanValue.False, TrileanValue.Unknown,
            TrileanValue.Unknown, TrileanValue.Unknown, TrileanValue.Unknown,
        };
        
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
        /// Creates a new trilean.
        /// </summary>
        /// <param name="value">
        /// The nullable boolean value. If the value is <c>null</c>, <see cref="TrileanValue.Unknown"/> will be assumed.
        /// </param>
        public Trilean(bool? value)
        {
            if (!value.HasValue)
                Value = TrileanValue.Unknown;
            else if (value.Value)
                Value = TrileanValue.True;
            else
                Value = TrileanValue.False;
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
        /// Converts the trilean to a nullable boolean, where null indicates the unknown state.
        /// </summary>
        /// <returns>The nullable boolean.</returns>
        public bool? ToNullableBoolean() => Value switch
        {
            TrileanValue.Unknown => null,
            TrileanValue.False => false,
            TrileanValue.True => true,
            _ => throw new ArgumentOutOfRangeException(nameof(Value))
        };
        
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
        /// Creates a new trilean.
        /// </summary>
        /// <param name="value">The trilean value.</param>
        public static implicit operator Trilean(bool? value) => new Trilean(value);

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

        /// <summary>
        /// Inverts the trilean value.
        /// </summary>
        /// <param name="value">The value to invert.</param>
        /// <returns>
        /// Returns true if the value is false, and vice versa. If unknown, the return value is also unknown.
        /// </returns>
        public static Trilean operator !(Trilean value) => value.Not();
        
        /// <summary>
        /// Inverts the trilean value.
        /// </summary>
        /// <returns>
        /// Returns true if the value is false, and vice versa. If unknown, the return value is also unknown.
        /// </returns>
        public Trilean Not() => Value switch
        {
            TrileanValue.False => True,
            TrileanValue.True => False,
            TrileanValue.Unknown => Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(Value))
        };

        /// <summary>
        /// Calculates the index within a binary operator lookup table.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>The index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetLookupTableIndex(TrileanValue row, TrileanValue column) => (int) row * 3 + (int) column;

        /// <summary>
        /// Computes the and between two trilean values.
        /// </summary>
        /// <param name="a">The left hand side of the binary operator.</param>
        /// <param name="b">The right hand side of the binary operator.</param>
        /// <returns>Returns true if both values are true. If not, returns unknown if at
        /// least one is true or unknown and the other is unknown, and false otherwise.</returns>
        public static Trilean operator &(Trilean a, Trilean b) => a.And(b);
        
        /// <summary>
        /// Computes the and between two trilean values.
        /// </summary>
        /// <param name="other">The other trilean value.</param>
        /// <returns>Returns true if both values are true. If not, returns unknown if at
        /// least one is true or unknown and the other is unknown, and false otherwise.</returns>
        public Trilean And(Trilean other) => AndTable[GetLookupTableIndex(Value, other.Value)];
        
        /// <summary>
        /// Computes the inclusive or between two trilean values.
        /// </summary>
        /// <param name="a">The left hand side of the binary operator.</param>
        /// <param name="b">The right hand side of the binary operator.</param>
        /// <returns>Returns true if at least one of the values is true. If neither are true, returns unknown if at
        /// least one is unknown, and false otherwise.</returns>
        public static Trilean operator |(Trilean a, Trilean b) => a.Or(b);
        
        /// <summary>
        /// Computes the inclusive or between two trilean values.
        /// </summary>
        /// <param name="other">The other trilean value.</param>
        /// <returns>Returns true if at least one of the values is true. If neither are true, returns unknown if at
        /// least one is unknown, and false otherwise.</returns>
        public Trilean Or(Trilean other) => OrTable[GetLookupTableIndex(Value, other.Value)];
        
        /// <summary>
        /// Computes the exclusive or between two trilean values.
        /// </summary>
        /// <param name="a">The left hand side of the binary operator.</param>
        /// <param name="b">The right hand side of the binary operator.</param>
        /// <returns>Returns true if the two trilean values are different. If at least one is unknown,
        /// the result is unknown.</returns>
        public static Trilean operator ^(Trilean a, Trilean b) => a.Xor(b);
        
        /// <summary>
        /// Computes the exclusive or between two trilean values.
        /// </summary>
        /// <param name="other">The other trilean value.</param>
        /// <returns>Returns true if the two trilean values are different. If at least one is unknown,
        /// the result is unknown.</returns>
        public Trilean Xor(Trilean other) => XorTable[GetLookupTableIndex(Value, other.Value)];

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