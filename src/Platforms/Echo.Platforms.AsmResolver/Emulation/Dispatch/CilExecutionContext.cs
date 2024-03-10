using System.Threading;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    /// <summary>
    /// Provides a context for evaluating CIL instructions.
    /// </summary>
    public class CilExecutionContext
    {
        /// <summary>
        /// Creates a new execution context for CIL instructions.
        /// </summary>
        /// <param name="thread">The parent thread the instruction is executed on.</param>
        /// <param name="cancellationToken">A token used for canceling the emulation.</param>
        public CilExecutionContext(CilThread thread, CancellationToken cancellationToken)
        {
            Thread = thread;
            CancellationToken = cancellationToken;
        }

        /// <summary>
        /// Gets the parent thread the instruction is executed on.
        /// </summary>
        public CilThread Thread
        {
            get;
        }

        /// <summary>
        /// Gets the parent machine the instruction is executed on.
        /// </summary>
        public CilVirtualMachine Machine => Thread.Machine;

        /// <summary>
        /// Gets the current active stack frame.
        /// </summary>
        public CallFrame CurrentFrame => Thread.CallStack.Peek();

        /// <summary>
        /// Gets a token used for canceling the emulation.
        /// </summary>
        public CancellationToken CancellationToken
        {
            get;
        }
    }
}