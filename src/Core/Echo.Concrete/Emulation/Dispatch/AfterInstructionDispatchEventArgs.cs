namespace Echo.Concrete.Emulation.Dispatch
{
    /// <summary>
    /// Provides arguments for describing an event that fires after an instruction was dispatched and executed. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that was executed.</typeparam>
    public class AfterInstructionDispatchEventArgs<TInstruction> : InstructionDispatchEventArgs<TInstruction>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AfterInstructionDispatchEventArgs{TInstruction}"/> class.
        /// </summary>
        /// <param name="context">The context in which the instruction was executed.</param>
        /// <param name="instruction">The instruction that was executed.</param>
        /// <param name="result">The produced result.</param>
        public AfterInstructionDispatchEventArgs(ExecutionContext context, TInstruction instruction, DispatchResult result)
            : base(context, instruction)
        {
            Result = result;
        }

        /// <summary>
        /// Gets the result that was produced after dispatching.
        /// </summary>
        public DispatchResult Result
        {
            get;
        }
    }
}