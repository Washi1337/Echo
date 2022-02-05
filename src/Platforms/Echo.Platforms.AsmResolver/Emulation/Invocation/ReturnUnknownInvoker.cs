using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    public sealed class ReturnUnknownInvoker : IMethodInvoker
    {
        public static ReturnUnknownInvoker Instance
        {
            get;
        } = new();

        private ReturnUnknownInvoker()
        {
        }
        
        public BitVector? Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
        {
            if (method.Signature!.ReturnsValue)
            {
                uint size = context.Machine.ValueFactory.GetTypeValueMemoryLayout(method.Signature.ReturnType).Size;
                return context.Machine.ValueFactory.BitVectorPool.Rent((int) (size*8), false);
            }

            return null;
        }
    }
}