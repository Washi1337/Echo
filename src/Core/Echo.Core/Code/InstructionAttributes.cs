using System;

namespace Echo.Core.Code
{
    /// <summary>
    /// Provides members for describing various properties of an instruction. 
    /// </summary>
    [Flags]
    public enum InstructionAttributes
    {
        /// <summary>
        /// Indicates the instruction does not have any specific attributes assigned to it.
        /// </summary>
        None,
        
        /// <summary>
        /// Indicates the instruction might branch out from the normal control flow to a different instruction. 
        /// </summary>
        CanBranch,
        
        /// <summary>
        /// Indicates the instruction terminates the current execution path.
        /// </summary>
        IsTerminator
    }
}