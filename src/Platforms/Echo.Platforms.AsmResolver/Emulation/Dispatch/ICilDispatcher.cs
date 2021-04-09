using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides members for dispatching instructions for a certain architecture. 
    /// </summary>
    public interface ICilDispatcher
    {
        /// <summary>
        /// Represents the event that is fired before an instruction is dispatched to an operation code handler.
        /// </summary>
        event EventHandler<BeforeInstructionDispatchEventArgs> BeforeInstructionDispatch;
        
        /// <summary>
        /// Represents the event that is fired after an instruction is dispatched and executed.
        /// </summary>
        event EventHandler<InstructionDispatchEventArgs> AfterInstructionDispatch;

        /// <summary>
        /// Dispatches the provided instruction to an operation code handler.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="instruction">The instruction to evaluate.</param>
        /// <returns>The dispatch result.</returns>
        DispatchResult Execute(CilExecutionContext context, CilInstruction instruction);
    }
}