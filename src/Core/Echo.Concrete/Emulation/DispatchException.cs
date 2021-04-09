using System;
using System.Runtime.Serialization;

namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Represents an exception that occurs during the dispatch phase of a virtual machine.
    /// </summary>
    [Serializable]
    public class DispatchException : EmulationException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DispatchException"/> class.
        /// </summary>
        public DispatchException()
            : this("An internal emulator error occured during the dispatch phase of the virtual machine.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DispatchException"/> class, with the provided message.
        /// </summary>
        /// <param name="message">The error message.</param> 
        public DispatchException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DispatchException"/> class, with the provided message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner cause of the error.</param>
        public DispatchException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <inheritdoc />
        protected DispatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}