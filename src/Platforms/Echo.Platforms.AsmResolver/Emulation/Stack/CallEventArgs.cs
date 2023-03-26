using System;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Provides a description of events related to a frame in a call stack. 
    /// </summary>
    public class CallEventArgs : EventArgs
    {
        internal CallEventArgs()
        {
            Frame = null!;
        }

        /// <summary>
        /// Gets the call frame related to the event.
        /// </summary>
        public CallFrame Frame
        {
            get;
            internal set;
        }
    }
}