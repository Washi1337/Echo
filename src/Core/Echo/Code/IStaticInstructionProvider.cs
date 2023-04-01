namespace Echo.Code
{
    /// <summary>
    /// Represents a collection of instructions that can be accessed by their offset.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that this collection provides.</typeparam>
    public interface IStaticInstructionProvider<TInstruction>
    {
        /// <summary>
        /// Gets the architecture describing the instructions exposed by this instruction provider.
        /// </summary>
        IInstructionSetArchitecture<TInstruction> Architecture
        {
            get;
        }

        /// <summary>
        /// Gets the instruction at the provided address.
        /// </summary>
        /// <param name="offset">The address of the instruction to get.</param>
        /// <returns>The instruction at the provided address.</returns>
        TInstruction GetInstructionAtOffset(long offset);
        
    }
}