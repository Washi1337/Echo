using System;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Describes an exception that was thrown by emulated CIL code.
    /// </summary>
    public class EmulatedException : Exception
    {
        /// <summary>
        /// Wraps the provided virtual exception object into an <see cref="EmulatedException"/> instance.
        /// </summary>
        /// <param name="exceptionObject">The handle to the virtual exception object.</param>
        public EmulatedException(ObjectHandle exceptionObject)
            : base($"An exception of type {exceptionObject.GetObjectType().FullName} was thrown by the emulated code.")
        {
            ExceptionObject = exceptionObject;
        }

        /// <summary>
        /// Gets the handle to the virtual exception object that was thrown by the emulated CIL code.
        /// </summary>
        public ObjectHandle ExceptionObject
        {
            get;
        }
    }
}