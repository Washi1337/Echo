namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Defines a capture group in a pattern.
    /// </summary>
    public abstract class CaptureGroup
    {
        internal CaptureGroup(string name)
        {
            Name = name;
        }
        
        /// <summary>
        /// Gets the name of the capture group.
        /// </summary>
        public string Name
        {
            get;
        }
    }

    /// <summary>
    /// Defines a capture group in a pattern of <typeparamref name="T"/> instances.
    /// </summary>
    /// <typeparam name="T">The type of objects to capture in the group.</typeparam>
    public class CaptureGroup<T> : CaptureGroup
    {
        /// <summary>
        /// Creates a new unnamed capture group.
        /// </summary>
        public CaptureGroup()
            : base(null)
        {
        }
        
        /// <summary>
        /// Creates a new named capture group.
        /// </summary>
        /// <param name="name">The name of the capture.</param>
        public CaptureGroup(string name)
            : base(name)
        {
        }

        /// <inheritdoc />
        public override string ToString() => Name ?? $"(Anonymous {typeof(T)} Group)";
    }
}