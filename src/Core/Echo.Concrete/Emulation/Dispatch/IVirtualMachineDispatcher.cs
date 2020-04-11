using System;

namespace Echo.Concrete.Emulation.Dispatch
{
    /// <summary>
    /// Provides members for dispatching instructions for a certain architecture. 
    /// </summary>
    /// <typeparam name="TInstruction"></typeparam>
    public interface IVirtualMachineDispatcher<TInstruction>
    {
        /// <summary>
        /// Represents the event that is fired before an instruction is dispatched to an operation code handler.
        /// </summary>
        event EventHandler<BeforeInstructionDispatchEventArgs<TInstruction>> BeforeInstructionDispatch;
        
        /// <summary>
        /// Represents the event that is fired after an instruction is dispatched and executed.
        /// </summary>
        event EventHandler<InstructionDispatchEventArgs<TInstruction>> AfterInstructionDispatch;

        /// <summary>
        /// Dispatches the provided instruction to an operation code handler.
        /// </summary>
        /// <param name="context">The context to execute the instruction in.</param>
        /// <param name="instruction">The instruction to evaluate.</param>
        /// <returns>The dispatch result.</returns>
        DispatchResult Execute(ExecutionContext context, TInstruction instruction);
    }
}