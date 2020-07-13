using System;

namespace Echo.Core.Code
{
    /// <summary>
    /// Provides members for describing various flow control properties of an instruction. 
    /// </summary>
    [Flags]
    public enum InstructionFlowControl
    {
        /// <summary>
        /// Indicates the instruction does not have any specific attributes assigned to it.
        /// </summary>
        Fallthrough = 1,
        
        /// <summary>
        /// Indicates the instruction might branch out from the normal control flow to a different instruction. 
        /// </summary>
        CanBranch = 2,
        
        /// <summary>
        /// Indicates the instruction terminates the current execution path.
        /// </summary>
        IsTerminator = 4,
    }
}