using System;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// The exception that occurs when a <see cref="CilVirtualMachine"/> encounters an internal error.
    /// </summary>
    public class CilEmulatorException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CilEmulatorException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CilEmulatorException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CilEmulatorException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="inner">The inner cause of the error.</param>
        public CilEmulatorException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}