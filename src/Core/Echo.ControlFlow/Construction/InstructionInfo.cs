using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.ControlFlow.Construction
{
    /// <summary>
    /// A tuple-like structure that associates a single instruction with a collection of successor
    /// instruction references.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to associate with successors.</typeparam>
    /// <remarks>
    /// This structure is used by graph builders to keep track of instructions and their successors to eventually build
    /// a control flow graph.
    /// </remarks>
    public struct InstructionInfo<TInstruction>
        where TInstruction : IInstruction
    {
        public InstructionInfo(TInstruction instruction, ICollection<SuccessorInfo> successors)
        {
            Instruction = instruction;
            Successors = successors;
        }
            
        /// <summary>
        /// Gets the instruction that was associated with a collection of successors.
        /// </summary>
        public TInstruction Instruction
        {
            get;
        }

        /// <summary>
        /// Gets a collection of successors associated to the instruction.
        /// </summary>
        public ICollection<SuccessorInfo> Successors
        {
            get;
        }
    }
}