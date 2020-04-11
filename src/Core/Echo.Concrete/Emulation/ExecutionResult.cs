using System;
using Echo.Concrete.Values;

namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Represents an aggregation of results that were produced during the execution of a virtual machine. 
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// Gets a value indicating whether the execution finished successfully.
        /// </summary>
        public bool IsSuccess => Exception is null;

        /// <summary>
        /// Gets or sets the exception that was thrown during the execution of the program (if any).
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the return value that was produced (if any).
        /// </summary>
        /// <remarks>
        /// A value of <c>null</c> indicates no value was produced.
        /// </remarks>
        public IConcreteValue ReturnValue
        {
            get;
            set;
        }
    }
}