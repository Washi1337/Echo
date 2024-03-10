using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Provides an implementation of an allocator that allocates new objects in the virtualized heap of the underlying
/// virtual machine. 
/// </summary>
public class VirtualHeapAllocator : IObjectAllocator
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="VirtualHeapAllocator"/> class.
    /// </summary>
    public static VirtualHeapAllocator Instance { get; } = new();

    /// <inheritdoc />
    public AllocationResult Allocate(CilExecutionContext context, IMethodDescriptor ctor, IList<BitVector> arguments)
    {
        var instanceType = ctor.DeclaringType;
        if (instanceType is null)
            return AllocationResult.Inconclusive();

        long address = context.Machine.Heap.AllocateObject(instanceType, true);
        return AllocationResult.Allocated(context.Machine.ValueFactory.RentNativeInteger(address));
    }
}