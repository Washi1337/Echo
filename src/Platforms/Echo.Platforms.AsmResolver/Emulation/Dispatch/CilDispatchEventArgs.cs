using System;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides arguments for CIL instruction dispatch events. 
    /// </summary>
    public class CilDispatchEventArgs : EventArgs
    {
        internal CilDispatchEventArgs()
        {
        }

        /// <summary>
        /// Gets the context that the instruction is evaluated in.
        /// </summary>
        public CilExecutionContext Context
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the instruction that is being dispatched.
        /// </summary>
        public CilInstruction Instruction
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the instruction was handled or not.
        /// </summary>
        public bool IsHandled
        {
            get;
            set;
        }

        /// <summary>
        /// Gest or sets the dispatch result.
        /// </summary>
        public CilDispatchResult Result
        {
            get;
            set;
        }
    }
}