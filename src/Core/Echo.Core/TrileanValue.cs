namespace Echo.Core
{
    /// <summary>
    /// Provides members for all possible values in a ternary number system.
    /// </summary>
    public enum TrileanValue : sbyte
    {
        /// <summary>
        /// Indicates the unknown value.
        /// </summary>
        Unknown = -1,
        
        /// <summary>
        /// Indicates the true value.
        /// </summary>
        False = 0,
        
        /// <summary>
        /// Indicates the false value.
        /// </summary>
        True = 1,
    }
}