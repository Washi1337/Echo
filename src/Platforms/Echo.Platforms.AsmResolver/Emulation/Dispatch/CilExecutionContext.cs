using System.Threading;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation.Dispatch
{
    public class CilExecutionContext
    {
        public CilExecutionContext(CilVirtualMachine machine, CancellationToken cancellationToken)
        {
            Machine = machine;
            CancellationToken = cancellationToken;
        }

        public CilVirtualMachine Machine
        {
            get;
        }

        public CallFrame CurrentFrame => Machine.CallStack.Peek();

        public CancellationToken CancellationToken
        {
            get;
        }
    }
}