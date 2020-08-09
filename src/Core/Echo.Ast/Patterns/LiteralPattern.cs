namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes a pattern that matches the input with a specific object. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LiteralPattern<T> : Pattern<T>
    {
        /// <summary>
        /// Creates a new literal pattern.
        /// </summary>
        /// <param name="o">The object to match with.</param>
        public LiteralPattern(T o)
        {
            Value = o;
        }

        /// <summary>
        /// Gets or sets the object that the input should match.
        /// </summary>
        public T Value
        {
            get;
            set;
        }

        /// <inheritdoc />
        protected override void MatchChildren(T input, MatchResult result)
        {
            // Note: we use the static object.Equals over the instance method to allow for null references to be matched.
            result.IsSuccess = Equals(input, Value);
        }

        /// <inheritdoc />
        public override string ToString() => Value?.ToString() ?? "null";

        /// <summary>
        /// Converts the provided literal to a pattern.
        /// </summary>
        /// <param name="value">The value to match.</param>
        /// <returns>The resulting pattern.</returns>
        public static implicit operator LiteralPattern<T>(T value) => new LiteralPattern<T>(value);
    }
}