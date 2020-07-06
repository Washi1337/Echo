using System;
using System.Runtime.Serialization;

namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Represents the exception that is thrown when a virtual machine encounters an error. 
    /// </summary>
    [Serializable]
    public class EmulationException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="EmulationException"/> class.
        /// </summary>
        public EmulationException()
            : this("An internal emulator exception occured.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EmulationException"/> class, with the provided message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public EmulationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="EmulationException"/> class, with the provided message
        /// and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner cause of the exception.</param>
        public EmulationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <inheritdoc />
        protected EmulationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}