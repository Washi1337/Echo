using System;

namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Provides arguments for describing the event that is fired upon the termination of a virtual machine. 
    /// </summary>
    public class ExecutionTerminatedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ExecutionTerminatedEventArgs"/> class.
        /// </summary>
        /// <param name="result">The results produced during the execution.</param>
        public ExecutionTerminatedEventArgs(ExecutionResult result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }
        
        /// <summary>
        /// Gets the results that were produced during the execution of the virtual machine.
        /// </summary>
        public ExecutionResult Result
        {
            get;
        }
    }
}