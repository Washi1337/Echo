using System;

namespace Echo.Concrete.Emulation.Dispatch
{
    /// <summary>
    /// Represents a result produced after dispatching an instruction to an operation code handler. 
    /// </summary>
    public struct DispatchResult
    {
        public bool Exit
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