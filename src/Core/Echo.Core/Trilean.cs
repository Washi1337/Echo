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
        /// Creates a new trilean.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        public static implicit operator Trilean(bool value) => new Trilean(value);
        
        /// <summary>
        /// Creates a new trilean.
        /// </summary>
        /// <param name="value">The trilean value.</param>
        public static implicit operator Trilean(TrileanValue value) => new Trilean(value);

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