using System;
using System.Runtime.Serialization;

namespace Echo.Concrete.Emulation.Dispatch
{
    /// <summary>
    /// Represents the exception that occurs upon the execution of an undefined instruction.
    /// </summary>
    [Serializable]
    public class UndefinedInstructionException : DispatchException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="UndefinedInstructionException"/>.
        /// </summary>
        public UndefinedInstructionException()
            : base($"Attempted to execute an undefined instruction.")
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UndefinedInstructionException"/>.
        /// </summary>
        /// <param name="offset">The offset of the undefined instruction.</param>
        public UndefinedInstructionException(long offset)
            : base($"Attempted to execute an undefined instruction at offset 0x{offset:X8}.")
        {
            Offset = offset;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UndefinedInstructionException"/>.
        /// </summary>
        /// <param name="message">The error message</param>
        public UndefinedInstructionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="UndefinedInstructionException"/>.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="inner">The inner cause of the exception.</param>
        public UndefinedInstructionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <inheritdoc />
        protected UndefinedInstructionException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
        
        /// <summary>
        /// Gets the offset of the undefined instruction that was attempted to be executed.
        /// </summary>
        public long Offset
        {
            get;
        }
    }
}