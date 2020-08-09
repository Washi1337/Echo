using System.Collections.Generic;

namespace Echo.Ast.Pattern
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
                tempResult.IsSuccess = true;
                tempResult.Captures.Clear();

                // Try matching the current option.
                option.Match(input, tempResult);

                // If successful, copy over captured objects.
                if (tempResult.IsSuccess)
                {
                    foreach (var entry in result.Captures)
                    {
                        foreach (var o in entry.Value)
                            result.AddCapturedObject(entry.Key, o);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override string ToString() => $"({string.Join(" | ", Options)})";
    }
}