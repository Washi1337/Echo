namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Provides type hints indicating how a value was pushed onto the stack.
    /// </summary>
    public enum StackSlotTypeHint
    {
        /// <summary>
        /// Indicates the value was pushed as an integer.
        /// </summary>
        Integer,
        
        /// <summary>
        /// Indicates the value was pushed as a floating point number.
        /// </summary>
        Float,
        
        /// <summary>
        /// Indicates the value was pushed as a custom structure.
        /// </summary>
        Structure
    }
}