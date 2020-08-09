using System;

namespace Echo.Ast.Pattern
{
    /// <summary>
    /// Defines a capture group in a pattern.
    /// </summary>
    public class CaptureGroup
    {
        /// <summary>
        /// Creates a new named capture group.
        /// </summary>
        /// <param name="name">The name of the capture.</param>
        public CaptureGroup(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        
        /// <summary>
        /// Gets the name of the capture group.
        /// </summary>
        public string Name
        {
            get;
        }
    }
}