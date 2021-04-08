using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides arguments for describing an event that fires before an instruction is dispatched and executed. 
    /// </summary>
    public class BeforeInstructionDispatchEventArgs : InstructionDispatchEventArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BeforeInstructionDispatchEventArgs"/> class.
        /// </summary>
        /// <param name="context">The context in which the instruction is being executed.</param>
        /// <param name="instruction">The instruction that is being executed.</param>
        public BeforeInstructionDispatchEventArgs(CilExecutionContext context, CilInstruction instruction)
            : base(context, instruction)
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the instruction is handled and should not be dispatched to the
        /// default operation handler.
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>true</c>, the <see cref="ResultOverride"/> property will be used as the
        /// final emulation result.
        /// </remarks>
        public bool Handled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the dispatch result when this instruction is handled externally. 
        /// </summary>
        /// <remarks>
        /// This value is ignored when the <see cref="Handled"/> property is set to <c>false</c>.
        /// </remarks>
        public DispatchResult ResultOverride
        {
            get;
            set;
        }
    }
}