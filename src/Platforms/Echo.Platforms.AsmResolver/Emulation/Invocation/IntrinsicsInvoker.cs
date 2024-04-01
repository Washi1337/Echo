using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Implements a method invoker that shims methods in the <c>System.Runtime.Intrinsics</c> namespace.
/// </summary>
public class IntrinsicsInvoker : IMethodInvoker
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="IntrinsicsInvoker"/> class.
    /// </summary>
    public static IntrinsicsInvoker Instance { get; } = new();
    
    /// <inheritdoc />
    public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        if (method is not { DeclaringType: {} declaringType, Name: {} name })
            return InvocationResult.Inconclusive();
        
        if (declaringType.Namespace != "System.Runtime.Intrinsics")
            return InvocationResult.Inconclusive();

        return name.Value switch
        {
            "get_IsHardwareAccelerated" => InvokeIsHardwareAccelerated(context),
            _ => InvocationResult.Inconclusive()
        };
    }
    
    private static InvocationResult InvokeIsHardwareAccelerated(CilExecutionContext context)
    {
        // We assume no hardware acceleration.
        return InvocationResult.StepOver(context.Machine.ValueFactory.RentBoolean(false));
    }
}