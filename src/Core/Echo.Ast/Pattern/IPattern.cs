namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Represents an object pattern.
    /// </summary>
    /// <typeparam name="T">The type of objects to match.</typeparam>
    public interface IPattern<in T>
    {
        /// <summary>
        /// Attempts to match and extract any captured groups from the given input.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="result">The buffer to store the extracted objects in.</param>
        void Match(T input, MatchResult result);
    }
}