using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Implements a method invoker that always steps over the requested method and returns a specific value.
/// </summary>
/// <param name="value">The value to return.</param>
public class ReturnConstantInvoker(BitVector value) : IMethodInvoker
{
    /// <summary>
    /// Gets the value to return.
    /// </summary>
    public BitVector Value { get; } = value;

    /// <inheritdoc />
    public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        return InvocationResult.StepOver(Value.Clone(context.Machine.ValueFactory.BitVectorPool));
    }
}