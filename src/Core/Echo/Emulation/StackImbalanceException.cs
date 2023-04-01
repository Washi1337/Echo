using System;

namespace Echo.Emulation
{
    /// <summary>
    /// The exception that is thrown when an inconsistency in the size of the stack was detected. Typically this
    /// exception occurs when two or more converging control flow paths have inconsistent stack sizes, or when
    /// either an insufficient or excessive amount of values were pushed onto the stack.
    /// </summary>
    public class StackImbalanceException : Exception
    {
        /// <summary>
        /// Creates a new stack imbalance exception.
        /// </summary>
        public StackImbalanceException()
            : this("Stack imbalance was detected.")
        {
        }
        
        /// <summary>
        /// Creates a new stack imbalance exception.
        /// </summary>
        /// <param name="offset">The offset where the stack inconsistency was detected.</param>
        public StackImbalanceException(long offset)
            : this($"Stack imbalance was detected at offset {offset:X8}.")
        {
        }

        /// <summary>
        /// Creates a new stack imbalance exception.
        /// </summary>
        /// <param name="message">The message of the error that occured.</param>
        public StackImbalanceException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new stack imbalance exception.
        /// </summary>
        /// <param name="message">The message of the error that occured.</param>
        /// <param name="innerException">The inner cause of the exception.</param>
        public StackImbalanceException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the offset where the stack inconsistency was detected.
        /// </summary>
        public long Offset
        {
            get;
        }
        
    }
}