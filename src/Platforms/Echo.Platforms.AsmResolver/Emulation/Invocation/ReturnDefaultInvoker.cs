using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    internal class ReturnDefaultInvoker : IMethodInvoker
    {
        internal static readonly ReturnDefaultInvoker ReturnUnknown = new(false);
        internal static readonly ReturnDefaultInvoker ReturnDefault = new(true);

        public ReturnDefaultInvoker(bool initialize)
        {
            Initialize = initialize;
        }

        public bool Initialize
        {
            get;
        }

        /// <inheritdoc />
        public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            BitVector? returnValue;
            if (!method.Signature!.ReturnsValue)
            {
                returnValue = null;
            }
            else
            {
                var factory = context.Machine.ValueFactory;
                uint size = factory.GetTypeValueMemoryLayout(method.Signature.ReturnType).Size;
                returnValue = factory.BitVectorPool.Rent((int) size * 8, Initialize);
            }

            return InvocationResult.StepOver(returnValue);
        }
    }
}