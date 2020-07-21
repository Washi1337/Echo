namespace Echo.Core.Code
{
    /// <summary>
    /// Provides members for describing the purity of instructions.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions.</typeparam>
    public interface IPurityClassifier<TInstruction>
    {
        /// <summary>
        /// Gets a value indicating whether a particular instruction is considered pure, that is, has no side effects.
        /// </summary>
        /// <param name="instruction">The instruction to classify.</param>
        /// <returns><c>true</c> if the instruction is pure, <c>false</c> if not, and <see cref="Trilean.Unknown"/> if
        /// this could not be determined.</returns>
        Trilean IsPure(in TInstruction instruction);
    }
}