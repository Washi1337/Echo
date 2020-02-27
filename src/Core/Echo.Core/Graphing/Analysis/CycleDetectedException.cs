using System;

namespace Echo.Core.Graphing.Analysis
{
    /// <summary>
    /// Represents the error that occurs when a cycle was found in a graph that is supposed to be acyclic. 
    /// </summary>
    public class CycleDetectedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CycleDetectedException"/> class.
        /// </summary>
        public CycleDetectedException()
            : this("A cycle was found in the graph.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CycleDetectedException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CycleDetectedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CycleDetectedException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception that was the cause of this exception.</param>
        public CycleDetectedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}