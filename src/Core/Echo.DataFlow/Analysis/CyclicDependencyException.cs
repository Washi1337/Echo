using System;

namespace Echo.DataFlow.Analysis
{
    /// <summary>
    /// Represents the exception that occurs when a cyclic dependency was detected in a data flow graph.
    /// </summary>
    public class CyclicDependencyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CyclicDependencyException"/> class.
        /// </summary>
        public CyclicDependencyException()
            : base("Cyclic dependency was detected.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CyclicDependencyException"/> class, with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CyclicDependencyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CyclicDependencyException"/> class, with the specified message
        /// and inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception that was the cause of this exception.</param>
        public CyclicDependencyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}