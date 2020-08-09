using System.Collections.Generic;

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
        /// <returns>The pattern.</returns>
        public static AnyPattern<T> Any() => new AnyPattern<T>();
        
        /// <summary>
        /// Creates a new pattern that matches the input with an exact object instance of the specified type.
        /// </summary>
        /// <param name="o">The instance to match with.</param>
        /// <returns></returns>
        public static LiteralPattern<T> Literal(T o) => new LiteralPattern<T>(o);
        
        /// <summary>
        /// Combines two patterns together into a single <see cref="OrPattern{T}"/>.
        /// </summary>
        /// <param name="a">The first pattern.</param>
        /// <param name="b">The second pattern.</param>
        /// <returns>The resulting pattern.</returns>
        /// <remarks>
        /// This method flattens all options into a single <see cref="OrPattern{T}"/>.  When a specified pattern is
        /// already an <see cref="OrPattern{T}"/>, the options of that particular pattern will be used instead of the
        /// <see cref="OrPattern{T}"/> itself. 
        /// </remarks>
        public static OrPattern<T> operator |(Pattern<T> a, Pattern<T> b)
        {
            var options = new List<Pattern<T>>();
            
            if (a is OrPattern<T> orA)
                options.AddRange(orA.Options);
            else
                options.Add(a);
            
            if (b is OrPattern<T> orB)
                options.AddRange(orB.Options);
            else
                options.Add(b);
            
            return new OrPattern<T>(options);
        } 
        
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
            return result;
        }

        /// <summary>
        /// Attempts to match and extract any captured groups from the given input.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="result">The buffer to store the extracted objects in.</param>
        public void Match(T input, MatchResult result)
        {
            MatchChildren(input, result);

            if (result.IsSuccess && CaptureGroup != null)
                result.AddCapturedObject(CaptureGroup, input);
        }

        /// <summary>
        /// Attempts to match and extract any captured groups from the given input's children.
        /// </summary>
        /// <param name="input">The input object.</param>
        /// <param name="result">The buffer to store the extracted objects in.</param>
        protected abstract void MatchChildren(T input, MatchResult result);

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