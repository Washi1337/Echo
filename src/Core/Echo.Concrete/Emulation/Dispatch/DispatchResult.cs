using System;

namespace Echo.Concrete.Emulation.Dispatch
{
    /// <summary>
    /// Represents a result produced after dispatching an instruction to an operation code handler. 
    /// </summary>
    public struct DispatchResult
    {
        /// <summary>
        /// Creates a new dispatch result indicating a successful dispatch and execution of the instruction. 
        /// </summary>
        /// <returns>The dispatch result.</returns>
        public static DispatchResult Success() => new DispatchResult();
        
        /// <summary>
        /// Creates a new dispatch result indicating an invalid program was detected. 
        /// </summary>
        /// <returns>The dispatch result.</returns>
        public static DispatchResult InvalidProgram() => new DispatchResult(new InvalidProgramException());

        /// <summary>
        /// Creates a new dispatch result with the provided exception.
        /// </summary>
        /// <param name="exception">The exception that occured during the execution of the instruction.</param>
        public DispatchResult(Exception exception)
        {
            HasTerminated = false;
            Exception = exception;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the execution of the program was terminated.
        /// </summary>
        public bool HasTerminated
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets a value indicating the execution of an instruction was successful.
        /// </summary>
        public bool IsSuccess => Exception is null;
        
        /// <summary>
        /// Gets or sets the exception that was thrown during the dispatch of the instruction (if any).
        /// </summary>
        public Exception Exception
        {
            get;
            set;
        }
    }
}