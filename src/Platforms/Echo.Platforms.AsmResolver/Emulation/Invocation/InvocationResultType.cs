namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides members describing the different types of invocation results that can be produced during a method
    /// invocation in a CIL virtual machine.
    /// </summary>
    public enum InvocationResultType
    {
        /// <summary>
        /// Indicates the invocation was not handled yet.
        /// </summary>
        Inconclusive,

        /// <summary>
        /// Indicates the invocation is handled as a step-in action.
        /// </summary>
        StepIn,
        
        /// <summary>
        /// Indicates the invocation is handled fully by the invoker.
        /// </summary>
        StepOver,

        /// <summary>
        /// Indicates the invocation is fully emulated by the invoker.
        /// </summary>
        FullyHandled,

        /// <summary>
        /// Indicates the invocation resulted in an error.
        /// </summary>
        Exception
    }
}