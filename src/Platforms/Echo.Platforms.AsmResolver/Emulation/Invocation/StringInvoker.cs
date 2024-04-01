using System;
using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Implements a method invoker that shims methods from the <see cref="System.String"/> class.
/// </summary>
public class StringInvoker : IMethodInvoker
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="StringInvoker"/> class.
    /// </summary>
    public static StringInvoker Instance { get; } = new();
    
    /// <inheritdoc />
    public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        if ((!method.DeclaringType?.IsTypeOf("System", "String") ?? true) || method.Signature is null)
            return InvocationResult.Inconclusive();

        switch (method.Name?.Value)
        {
            case "FastAllocateString":
                return InvokeFastAllocateString(context, arguments);
            
            default:
                return InvocationResult.Inconclusive();
        }
    }

    private static InvocationResult InvokeFastAllocateString(CilExecutionContext context, IList<BitVector> arguments)
    {
        var length = arguments[0];
        if (!length.IsFullyKnown)
            throw new CilEmulatorException("Cannot allocate a string with an unknown length.");
        
        long address = context.Machine.Heap.AllocateString(length.AsSpan().I32, true);
        var result = context.Machine.ValueFactory.RentNativeInteger(address);
        
        return InvocationResult.StepOver(result);
    }
}