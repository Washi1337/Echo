using System.Collections.Generic;
using System.Runtime.InteropServices;
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
            case "Intern":
                return InvokeIntern(context, arguments);
            case "IsInterned":
                return InvokeIsInterned(context, arguments);
                
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

    private static unsafe InvocationResult InvokeIntern(CilExecutionContext context, IList<BitVector> arguments)
    {
        var strArgument = arguments[0];
        if (!strArgument.IsFullyKnown)
            throw new CilEmulatorException("Cannot intern an unknown string.");
        
        var strHandle = strArgument.AsObjectHandle(context.Machine);
        if (strHandle.IsNull)
            throw new CilEmulatorException("Cannot intern a null string.");

        var strData = strHandle.ReadStringData();
        if (!strData.IsFullyKnown)
            throw new CilEmulatorException("Cannot intern an unknown string.");
        
        var chars = MemoryMarshal.Cast<byte, char>(strData.Bits);
        fixed (char* ptr = chars)
        {
            string str = new(ptr, 0, chars.Length);
            long internedAddress = context.Machine.Heap.GetInternedString(str);
            return InvocationResult.StepOver(context.Machine.ValueFactory.RentNativeInteger(internedAddress));
        }
    }

    private static unsafe InvocationResult InvokeIsInterned(CilExecutionContext context, IList<BitVector> arguments)
    {
        var strArgument = arguments[0];
        if (!strArgument.IsFullyKnown)
            throw new CilEmulatorException("Cannot check intern status of an unknown string.");
        
        var strHandle = strArgument.AsObjectHandle(context.Machine);
        if (strHandle.IsNull)
            throw new CilEmulatorException("Cannot check intern status of a null string.");

        var strData = strHandle.ReadStringData();
        if (!strData.IsFullyKnown)
            throw new CilEmulatorException("Cannot check intern status of an unknown string.");
        
        var chars = MemoryMarshal.Cast<byte, char>(strData.Bits);
        fixed (char* ptr = chars)
        {
            string str = new(ptr, 0, chars.Length);
            if (context.Machine.Heap.TryGetInternedString(str, out long internedAddress))
                return InvocationResult.StepOver(context.Machine.ValueFactory.RentNativeInteger(internedAddress));
        
            return InvocationResult.StepOver(null);
        }
    }
}