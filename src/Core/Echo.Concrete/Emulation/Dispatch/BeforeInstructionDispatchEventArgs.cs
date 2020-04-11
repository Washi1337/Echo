namespace Echo.Concrete.Emulation.Dispatch
{
    /// <summary>
    /// Provides arguments for describing an event that fires before an instruction is dispatched and executed. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is being executed.</typeparam>
    public class BeforeInstructionDispatchEventArgs<TInstruction> : InstructionDispatchEventArgs<TInstruction>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BeforeInstructionDispatchEventArgs{TInstruction}"/> class.
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        public BeforeInstructionDispatchEventArgs(ExecutionContext context, TInstruction instruction)
            : base(context, instruction)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the instruction is handled and should not be dispatched to the
        /// default operation handler.
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
    }
}