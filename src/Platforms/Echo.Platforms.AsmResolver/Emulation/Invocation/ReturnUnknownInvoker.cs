using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Provides an implementation for a <see cref="IMethodInvoker"/> that always succeeds and returns an unknown value. 
    /// </summary>
    public sealed class ReturnUnknownInvoker : IMethodInvoker
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="ReturnUnknownInvoker"/> class.
        /// </summary>
        public static ReturnUnknownInvoker Instance
        {
            get;
        } = new();

        private ReturnUnknownInvoker()
        {
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
                uint size = context.Machine.ValueFactory.GetTypeValueMemoryLayout(method.Signature.ReturnType).Size;
                returnValue = context.Machine.ValueFactory.BitVectorPool.Rent((int) size * 8, false);
            }

            return InvocationResult.Success(returnValue);
        }
    }
}