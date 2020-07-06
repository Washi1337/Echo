using System;
using Echo.Concrete.Values;
using Echo.Core.Emulation;

namespace Echo.Concrete.Emulation.Dispatch
{
    /// <summary>
    /// Provides arguments for describing events related to instruction dispatch.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that is dispatched.</typeparam>
    public class InstructionDispatchEventArgs<TInstruction> : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="InstructionDispatchEventArgs{TInstruction}"/> class.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed.</param>
        /// <param name="instruction">The instruction to execute.</param>
        public InstructionDispatchEventArgs(ExecutionContext context, TInstruction instruction)
        {
            Context = context;
            Instruction = instruction;
        }

        /// <summary>
        /// Gets the context in which the instruction is or was executed.
        /// </summary>
        public ExecutionContext Context
        {
            get;
        } 
        
        /// <summary>
        /// Gets the instruction that was executed or to execute. 
        /// </summary>
        public TInstruction Instruction
        {
            get;
        }
    }
}