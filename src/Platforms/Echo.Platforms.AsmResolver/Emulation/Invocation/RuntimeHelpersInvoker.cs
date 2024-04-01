using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AsmResolver;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Implements a method invoker that shims methods from the <see cref="RuntimeHelpers"/> class.
/// </summary>
public class RuntimeHelpersInvoker : IMethodInvoker
{
    /// <summary>
    /// Gets the singleton instance for the <see cref="RuntimeHelpersInvoker"/> class.
    /// </summary>
    public static RuntimeHelpersInvoker Instance { get; } = new();
    
    /// <inheritdoc />
    public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        if (method is not {DeclaringType: {} declaringType, Name: {} name})
            return InvocationResult.Inconclusive();
        
        if (!declaringType.IsTypeOf("System.Runtime.CompilerServices", "RuntimeHelpers"))
            return InvocationResult.Inconclusive();

        return name.Value switch
        {
            "IsReferenceOrContainsReferences" => InvokeIsReferenceOrContainsReferences(context, method),
            "InitializeArray" => InvokeInitializeArray(context, arguments),
            _ => InvocationResult.Inconclusive()
        };
    }

    private static InvocationResult InvokeIsReferenceOrContainsReferences(CilExecutionContext context, IMethodDescriptor method)
    {
        if (method is not MethodSpecification { Signature.TypeArguments: { Count: 1 } typeArguments })
            return InvocationResult.Inconclusive();
        
        // TODO: This is inaccurate (feature-blocked by https://github.com/Washi1337/AsmResolver/issues/530).
        bool result = !typeArguments[0].IsValueType;
        
        return InvocationResult.StepOver(context.Machine.ValueFactory.RentBoolean(result));
    }
    
    private static InvocationResult InvokeInitializeArray(CilExecutionContext context, IList<BitVector> arguments)
    {
        // Read parameters.
        var array = arguments[0].AsObjectHandle(context.Machine);
        var fieldHandle = arguments[1].AsStructHandle(context.Machine);

        // Resolve the field handle to a field descriptor.
        if (!context.Machine.ValueFactory.ClrMockMemory.Fields.TryGetObject(fieldHandle.Address, out var field))
            return InvocationResult.Inconclusive();

        // Resole the field behind the field descriptor.
        var definition = field!.Resolve();

        // Read the data.
        if (definition?.FieldRva is not IReadableSegment segment)
            return InvocationResult.Inconclusive();
        array.WriteArrayData(segment.ToArray());

        return InvocationResult.StepOver(null);
    }
}