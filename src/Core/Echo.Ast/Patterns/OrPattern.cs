using System.Collections.Generic;

namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes an object pattern that matches the input to a number of pattern options.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OrPattern<T> : Pattern<T>
    {
        /// <summary>
        /// Creates a new or pattern.
        /// </summary>
        /// <param name="options">The options to choose from.</param>
        public OrPattern(params Pattern<T>[] options)
        {
            foreach (var option in options)
                Options.Add(option);
        }
        
        /// <summary>
        /// Creates a new or pattern.
        /// </summary>
        /// <param name="options">The options to choose from.</param>
        public OrPattern(IEnumerable<Pattern<T>> options)
        {
            foreach (var option in options)
                Options.Add(option);
        }
        
        /// <summary>
        /// Describes all possible patterns that the input object might match with.
        /// </summary>
        public IList<Pattern<T>> Options
        {
            get;
        } = new List<Pattern<T>>();

        /// <inheritdoc />
        protected override void MatchChildren(T input, MatchResult result)
        {
            var tempResult = new MatchResult();
            
            foreach (var option in Options)
            {
                // Assume clean slate.
                tempResult.Clear();

                // Try matching the current option.
                option.Match(input, tempResult);

                // If successful, copy over captured objects.
                if (tempResult.IsSuccess)
                {
                    result.MergeWith(tempResult);
                    return;
                }
            }

            result.IsSuccess = false;
        }

        /// <inheritdoc />
        public override OrPattern<T> OrElse(Pattern<T> alternative)
        {
            var options = new List<Pattern<T>>(Options);

            if (alternative is OrPattern<T> alternativeOr)
                options.AddRange(alternativeOr.Options);
            else
                options.Add(alternative);

            return new OrPattern<T>(options);
        }

        /// <inheritdoc />
        public override string ToString() => $"[{string.Join(" | ", Options)}]";
    }
}