using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides arguments for describing an event that fires after an instruction was dispatched and executed. 
    /// </summary>
    public class AfterInstructionDispatchEventArgs : InstructionDispatchEventArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AfterInstructionDispatchEventArgs"/> class.
        /// </summary>
        /// <param name="context">The context in which the instruction was executed.</param>
        /// <param name="instruction">The instruction that was executed.</param>
        /// <param name="result">The produced result.</param>
        public AfterInstructionDispatchEventArgs(CilExecutionContext context, CilInstruction instruction, DispatchResult result)
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