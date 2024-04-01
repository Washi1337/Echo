using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Implements a method invoker that shims methods in the <see cref="System.Runtime.InteropServices.MemoryMarshal"/> class.
/// </summary>
public class MemoryMarshalInvoker : IMethodInvoker
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="MemoryMarshalInvoker"/> class.
    /// </summary>
    public static MemoryMarshalInvoker Instance { get; } = new();
    
    /// <inheritdoc />
    public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        if (method is not { DeclaringType: { } declaringType, Name: { } name })
            return InvocationResult.Inconclusive();
        
        if (!declaringType.IsTypeOf("System.Runtime.InteropServices", "MemoryMarshal"))
            return InvocationResult.Inconclusive();

        return name.Value switch
        {
            "GetArrayDataReference" => InvokeGetArrayDataReference(context, arguments),
            _ => InvocationResult.Inconclusive()
        };
    }

    private static InvocationResult InvokeGetArrayDataReference(CilExecutionContext context, IList<BitVector> arguments)
    {
        var arrayObject = arguments[0].AsObjectHandle(context.Machine);
        long result = arrayObject.Address + context.Machine.ValueFactory.ArrayHeaderSize;
        return InvocationResult.StepOver(context.Machine.ValueFactory.RentNativeInteger(result));
    }
}