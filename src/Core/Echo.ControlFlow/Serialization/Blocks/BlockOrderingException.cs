using System;

namespace Echo.ControlFlow.Serialization.Blocks
{
    /// <summary>
    /// Represents an exception that occurs during the sorting of nodes in a control flow graph.
    /// </summary>
    [Serializable]
    public class BlockOrderingException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BlockOrderingException"/> class.
        /// </summary>
        public BlockOrderingException()
            : base("Could not sort the nodes in the control flow graph.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BlockOrderingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public BlockOrderingException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BlockOrderingException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner cause of the exception.</param>
        public BlockOrderingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}