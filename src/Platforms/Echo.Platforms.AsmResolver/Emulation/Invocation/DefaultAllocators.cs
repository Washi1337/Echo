namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Provides methods for constructing object allocators using a set of default allocators implementations. 
/// </summary>
public static class DefaultAllocators
{
    /// <summary>
    /// Gets an object allocator that returns unknown addresses. 
    /// </summary>
    public static UnknownAddressAllocator UnknownAddress => UnknownAddressAllocator.Instance;
    
    /// <summary>
    /// Gets an allocator that allocates objects in the virtualized heap of the underlying virtual machine.
    /// </summary>
    public static VirtualHeapAllocator VirtualHeap => VirtualHeapAllocator.Instance;

    /// <summary>
    /// Gets an allocator that handles System.String constructors.
    /// </summary>
    public static StringAllocator String => StringAllocator.Instance;
    
    /// <summary>
    /// Chains the first object allocator with the provided object allocator in such a way that if the result of the
    /// first allocator is inconclusive, the second allocator will be used as a fallback allocator.  
    /// </summary>
    /// <param name="self">The first object allocator</param>
    /// <param name="other">The fallback object allocator</param>
    /// <returns>The constructed allocator chain.</returns>
    public static ObjectAllocatorChain WithFallback(this IObjectAllocator self, IObjectAllocator other)
    {
        if (self is not ObjectAllocatorChain chain)
        {
            chain = new ObjectAllocatorChain();
            chain.Allocators.Add(self);
        }

        chain.Allocators.Add(other);
        return chain;
    }

}