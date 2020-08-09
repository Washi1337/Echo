namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Represents an object pattern.
    /// </summary>
    /// <typeparam name="T">The type of objects to match.</typeparam>
    public abstract class Pattern<T>
    {
        /// <summary>
        /// Creates a new pattern that matches any object instance of the specified type.
        /// </summary>
        /// <returns>The type of objects.</returns>
        public static AnyPattern<T> Any() => new AnyPattern<T>();
        
        /// <summary>
        /// Gets or sets the capture group this pattern was assigned to.
        /// </summary>
        public CaptureGroup CaptureGroup
        {
            get;
            set;
        }

        /// <summary>
        /// Attempts to match and extract any captured groups from the given input.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <returns>The extracted objects.</returns>
        public MatchResult Match(T input)
        {
            var result = new MatchResult();
            Match(input, result);

            if (result.IsSuccess && CaptureGroup != null)
                result.AddCapturedObject(CaptureGroup, input);

            return result;
        }

        /// <summary>
        /// Attempts to match and extract any captured groups from the given input.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="result">The buffer to store the extracted objects in.</param>
        protected abstract void Match(T input, MatchResult result);

        /// <summary>
        /// When the pattern matches successfully, puts the the matched object in the provided capture group.
        /// </summary>
        /// <param name="captureGroup">The capture group to add the object to.</param>
        /// <returns>The pattern.</returns>
        public Pattern<T> Capture(CaptureGroup captureGroup)
        {
            CaptureGroup = captureGroup;
            return this;
        }
    }
}