using System.Collections.Generic;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes a pattern for sequences of objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to match.</typeparam>
    public class SequencePattern<T> 
    {
        /// <summary>
        /// Creates a new sequence pattern.
        /// </summary>
        /// <param name="elements">The patterns describing the elements.</param>
        public SequencePattern(params Pattern<T>[] elements)
        {
            Elements = new List<Pattern<T>>(elements);
        }
        
        /// <summary>
        /// Creates a new sequence pattern.
        /// </summary>
        /// <param name="elements">The patterns describing the elements.</param>
        public SequencePattern(IEnumerable<Pattern<T>> elements)
        {
            Elements = new List<Pattern<T>>(elements);
        }
        
        /// <summary>
        /// Gets the patterns describing the elements of the list.
        /// </summary>
        public IList<Pattern<T>> Elements
        {
            get;
        }
        
        /// <summary>
        /// Attempts to match and extract any captured groups from the given input.
        /// </summary>
        /// <param name="input">The input list.</param>
        /// <returns>The extracted objects.</returns>
        public MatchResult Match(IList<T> input)
        {
            var result = new MatchResult();
            Match(input, result);
            return result;
        }

        /// <summary>
        /// Attempts to match and extract any captured groups from the given input.
        /// </summary>
        /// <param name="input">The input list.</param>
        /// <param name="result">The buffer to store the extracted objects in.</param>
        public void Match(IList<T> input, MatchResult result)
        {
            MatchChildren(input, result);
        }
        
        private void MatchChildren(IList<T> input, MatchResult result)
        {
            if (input.Count != Elements.Count)
            {
                result.IsSuccess = false;
                return;
            }
            
            for (int i = 0; i < Elements.Count && result.IsSuccess; i++)
                Elements[i].Match(input[i], result);
        }

        /// <inheritdoc />
        public override string ToString() => string.Join(", ", Elements);

        /// <summary>
        /// Concatenates two patterns together into a single <see cref="SequencePattern{T}"/>.
        /// </summary>
        /// <param name="a">The first pattern.</param>
        /// <param name="b">The second pattern.</param>
        /// <returns>The resulting pattern.</returns>
        public static SequencePattern<T> operator +(SequencePattern<T> a, Pattern<T> b) => a.FollowedBy(b);
        
        /// <summary>
        /// Constructs a new sequence pattern that matches on sequences of objects, starting with the current list of
        /// objects, followed by one extra object pattern. 
        /// </summary>
        /// <param name="pattern">The next expected object.</param>
        /// <returns>The resulting pattern.</returns>
        public SequencePattern<T> FollowedBy(Pattern<T> pattern)
        {
            return new SequencePattern<T>(new List<Pattern<T>>(Elements)
            {
                pattern
            });
        }
        
        /// <summary>
        /// Concatenates two patterns together into a single <see cref="SequencePattern{T}"/>.
        /// </summary>
        /// <param name="a">The first pattern.</param>
        /// <param name="b">The second pattern.</param>
        /// <returns>The resulting pattern.</returns>
        public static SequencePattern<T> operator +(SequencePattern<T> a, SequencePattern<T> b) => a.FollowedBy(b);
        
        /// <summary>
        /// Constructs a new sequence pattern that matches on sequences of objects, starting with the current list of
        /// objects, followed by the patterns described in the provided sequence pattern. 
        /// </summary>
        /// <param name="pattern">The next expected objects.</param>
        /// <returns>The resulting pattern.</returns>
        public SequencePattern<T> FollowedBy(SequencePattern<T> pattern)
        {
            var elements = new List<Pattern<T>>(Elements);
            elements.AddRange(pattern.Elements);
            return new SequencePattern<T>(elements);
        }
        
        /// <summary>
        /// Constructs a new sequence pattern that matches on sequences of objects, starting with the current list of
        /// objects, followed by the patterns described in the provided sequence pattern. 
        /// </summary>
        /// <param name="patterns">The next expected objects.</param>
        /// <returns>The resulting pattern.</returns>
        public SequencePattern<T> FollowedBy(params Pattern<T>[] patterns)
        {
            var elements = new List<Pattern<T>>(Elements);
            elements.AddRange(patterns);
            return new SequencePattern<T>(elements);
        }
    }
}