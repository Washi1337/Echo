using System;
using AsmResolver.PE.DotNet.Cil;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides arguments for describing events related to instruction dispatch.
    /// </summary>
    public class InstructionDispatchEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="InstructionDispatchEventArgs"/> class.
        /// </summary>
        /// <param name="context">The context in which the instruction is executed.</param>
        /// <param name="instruction">The instruction to execute.</param>
        public InstructionDispatchEventArgs(CilExecutionContext context, CilInstruction instruction)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Instruction = instruction ?? throw new ArgumentNullException(nameof(instruction));
        }

        /// <summary>
        /// Gets the context in which the instruction is or was executed.
        /// </summary>
        public CilExecutionContext Context
        {
            get;
        } 
        
        /// <summary>
        /// Gets the instruction that was executed or to execute. 
        /// </summary>
        public CilInstruction Instruction
        {
            get;
        }
    }
}