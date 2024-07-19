using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Implements a method invoker that shims methods from the <see cref="System.Runtime.CompilerServices.Unsafe"/> class.
/// </summary>
public class UnsafeInvoker : IMethodInvoker
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="UnsafeInvoker"/> class.
    /// </summary>
    public static UnsafeInvoker Instance { get; } = new();

    /// <inheritdoc />
    public InvocationResult Invoke(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        if (method is not { Name: { } name, DeclaringType: { } declaringType, Signature: { } signature })
            return InvocationResult.Inconclusive();

        if (!declaringType.IsTypeOf("System.Runtime.CompilerServices", "Unsafe")
            && !declaringType.IsTypeOf("Internal.Runtime.CompilerServices", "Unsafe"))
        {
            return InvocationResult.Inconclusive();
        }

        // TODO: Add handlers for the remaining methods in the Unsafe class.
        switch (signature.ParameterTypes.Count)
        {
            case 1:
                return name.Value switch
                {
                    "As" => InvokeAsOrAsRef(context, arguments),
                    "AsRef" => InvokeAsOrAsRef(context, arguments),
                    "Read" => InvokeReadUnaligned(context, method, arguments),
                    "ReadUnaligned" => InvokeReadUnaligned(context, method, arguments),
                    _ => InvocationResult.Inconclusive()
                };
            
            case 2:
                return name.Value switch
                {
                    "AreSame" => InvokeAreSame(context, arguments),
                    "Add" => InvokeAdd(context, method, arguments),
                    "AddByteOffset" => InvokeAddByteOffset(context, arguments),
                    "ByteOffset" => InvokeByteOffset(context, arguments),
                    _ => InvocationResult.Inconclusive()
                };
            
            default:
                return InvocationResult.Inconclusive();
        }
    }
    
    private static InvocationResult InvokeAsOrAsRef(CilExecutionContext context, IList<BitVector> arguments)
    {
        // We don't do any GC tracking, thus returning the same input reference suffices.
        return InvocationResult.StepOver(arguments[0].Clone(context.Machine.ValueFactory.BitVectorPool));
    }

    private static InvocationResult InvokeAreSame(CilExecutionContext context, IList<BitVector> arguments)
    {
        var comparison = arguments[0].AsSpan().IsEqualTo(arguments[1]);
        var result = context.Machine.ValueFactory.RentBoolean(comparison);
        return InvocationResult.StepOver(result);
    }

    private static InvocationResult InvokeAddByteOffset(CilExecutionContext context, IList<BitVector> arguments)
    {
        var result = arguments[0].Clone(context.Machine.ValueFactory.BitVectorPool);
        result.AsSpan().IntegerAdd(arguments[1]);
        return InvocationResult.StepOver(result);
    }
    
    private static InvocationResult InvokeByteOffset(CilExecutionContext context, IList<BitVector> arguments)
    {
        var result = arguments[1].Clone(context.Machine.ValueFactory.BitVectorPool);
        result.AsSpan().IntegerSubtract(arguments[0]);
        return InvocationResult.StepOver(result);
    }

    private static InvocationResult InvokeAdd(CilExecutionContext context, IMethodDescriptor method, IList<BitVector> arguments)
    {
        if (method is not MethodSpecification { Signature.TypeArguments: { Count: 1 } typeArguments })
            throw new CilEmulatorException("Expected a method specification with a single type argument.");

        var valueFactory = context.Machine.ValueFactory;
        var pool = valueFactory.BitVectorPool;
        
        // Determine the size of a single element.
        uint elementSize = valueFactory.GetTypeValueMemoryLayout(typeArguments[0]).Size;
        var elementSizeVector = valueFactory.RentNativeInteger(elementSize);
        
        // We need to resize the offset as Unsafe.Add<T> accepts both a nint and an int32 as offset.
        var elementOffset = arguments[1].Resize((int)(valueFactory.PointerSize * 8), true, pool);
        
        // The offset is the index multiplied by its element size.
        elementOffset.AsSpan().IntegerMultiply(elementSizeVector);
        
        // Add the offset to the start address.
        var source = arguments[0].Clone(pool);
        source.AsSpan().IntegerAdd(elementOffset);
        
        // Return temporary vectors.
        pool.Return(elementSizeVector);
        pool.Return(elementOffset);

        return InvocationResult.StepOver(source);
    }

    private static InvocationResult InvokeReadUnaligned(
        CilExecutionContext context, 
        IMethodDescriptor method,
        IList<BitVector> arguments)
    {
        // Should be a generic instance method.
        if (method is not MethodSpecification { Signature: { } signature })
            return InvocationResult.Inconclusive();

        // Pointer should be known to be able to read from it.
        if (!arguments[0].IsFullyKnown)
            return InvocationResult.Inconclusive();

        // Concretize pointer.
        long resolvedAddress = arguments[0].AsSpan().ReadNativeInteger(context.Machine.Is32Bit);

        // Read from pointer.
        var result = context.Machine.ValueFactory.CreateValue(signature.TypeArguments[0], false);
        context.Machine.Memory.Read(resolvedAddress, result);

        return InvocationResult.StepOver(result);
    }
}