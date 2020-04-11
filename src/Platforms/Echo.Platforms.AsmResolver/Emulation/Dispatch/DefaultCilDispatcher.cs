using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides a default implementation for a CIL operation code handler dispatcher.
    /// </summary>
    public class DefaultCilDispatcher : IVirtualMachineDispatcher<CilInstruction>
    {
        /// <inheritdoc />
        public event EventHandler<BeforeInstructionDispatchEventArgs<CilInstruction>> BeforeInstructionDispatch;
        
        /// <inheritdoc />
        public event EventHandler<InstructionDispatchEventArgs<CilInstruction>> AfterInstructionDispatch;
        
        /// <inheritdoc />
        public DispatchResult Execute(ExecutionContext context, CilInstruction instruction)
        {
            var eventArgs = new BeforeInstructionDispatchEventArgs<CilInstruction>(context, instruction);
            OnBeforeInstructionDispatch(eventArgs);

            DispatchResult result;
            if (eventArgs.Handled)
            {
                result = eventArgs.ResultOverride ?? throw new DispatchException(
                    "No result was provided for the evaluation of a custom handled instruction.");
            }
            else
            {
                var handler = GetOpCodeHandler(instruction);
                result = handler.Execute(context, instruction);
            }

            OnAfterInstructionDispatch(new AfterInstructionDispatchEventArgs<CilInstruction>(context, instruction, result));
            return result;
        }

        /// <summary>
        /// Obtains the operation code handler for the provided instruction. 
        /// </summary>
        /// <param name="instruction">The instruction to get the handler for.</param>
        /// <returns>The operation code handler.</returns>
        /// <exception cref="UndefinedInstructionException">Occurs when the instruction is invalid or unsupported.</exception>
        protected virtual ICilOpCodeHandler GetOpCodeHandler(CilInstruction instruction)
        {
            // TODO:
            throw new UndefinedInstructionException(instruction.Offset);
        } 

        /// <summary>
        /// Invoked when an instruction is about to be dispatched. 
        /// </summary>
        /// <param name="e">The arguments describing the event.</param>
        protected virtual void OnBeforeInstructionDispatch(BeforeInstructionDispatchEventArgs<CilInstruction> e)
        {
            BeforeInstructionDispatch?.Invoke(this, e);
        }

        /// <summary>
        /// Invoked when an instruction is about to be dispatched. 
        /// </summary>
        /// <param name="e">The arguments describing the event.</param>
        protected virtual void OnAfterInstructionDispatch(InstructionDispatchEventArgs<CilInstruction> e)
        {
            AfterInstructionDispatch?.Invoke(this, e);
        }
    }
}