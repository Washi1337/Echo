namespace Echo.Core.Values
{
    /// <summary>
    /// Provides a base for all virtualized values.
    /// </summary>
    public interface IValue
    {
        /// <summary>
        /// Gets a value indicating whether all bits of the value are fully known or not. 
        /// </summary>
        bool IsKnown
        {
            get;
        }   
        
        /// <summary>
        /// Gets the number of bytes this value uses to represent itself in memory.
        /// </summary>
        int Size
        {
            get;
        }

        /// <summary>
        /// Creates a shallow copy of the value.
        /// </summary>
        /// <returns>The copied value.</returns>
        IValue Copy();
    }
}