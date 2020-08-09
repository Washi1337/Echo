namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Describes a pattern that matches any kind of instance of a specified type.
    /// </summary>
    /// <typeparam name="T">The type of objects to match.</typeparam>
    public class AnyPattern<T> : Pattern<T>
    {
        /// <inheritdoc />
        protected override void MatchChildren(T input, MatchResult result)
        {
        }

        /// <inheritdoc />
        public override string ToString() => "?";
    }
}