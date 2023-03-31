using System.Collections.Generic;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    internal class ReturnDefaultInvoker : IMethodInvoker
    {
        internal static readonly ReturnDefaultInvoker ReturnUnknown = new(false);
        internal static readonly ReturnDefaultInvoker ReturnDefault = new(true);
        private readonly bool _initialize;

        private ReturnDefaultInvoker(bool initialize)
        {
            _initialize = initialize;
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
                if (method.Signature.ReturnType.StripModifiers().ElementType != ElementType.Boolean)
                {
                    // Fully initialize or fully mark unknown.
                    returnValue = factory.RentValue(method.Signature.ReturnType, _initialize);
                }
                else
                {
                    // For booleans, we only set the LSB to unknown if necessary.
                    returnValue = factory.RentValue(method.Signature.ReturnType, true);
                    if (!_initialize)
                    {
                        var span = returnValue.AsSpan();
                        span[0] = Trilean.Unknown;
                    }
                }
            }

            return InvocationResult.StepOver(returnValue);
        }
    }
}