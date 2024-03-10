using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Provides an implementation of an allocator that always returns unknown addresses for object allocation requests. 
/// </summary>
public class UnknownAddressAllocator : IObjectAllocator
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="UnknownAddressAllocator"/> class.
    /// </summary>
    public static UnknownAddressAllocator Instance { get; } = new();
    
    /// <inheritdoc />
    public AllocationResult Allocate(CilExecutionContext context, IMethodDescriptor ctor, IList<BitVector> arguments)
    {
        var factory = context.Machine.ValueFactory;
        var address = factory.RentNativeInteger(false);
        return AllocationResult.Allocated(address);
    }
}