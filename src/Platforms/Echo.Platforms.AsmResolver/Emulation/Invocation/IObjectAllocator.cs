using System.Collections.Generic;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation;

/// <summary>
/// Provides members for emulating object allocation.
/// </summary>
public interface IObjectAllocator
{
    /// <summary>
    /// Allocates a new object with the provided constructor and arguments.
    /// </summary>
    /// <param name="context">The execution context the call originates from.</param>
    /// <param name="ctor">The constructor to invoke after allocation.</param>
    /// <param name="arguments">The arguments to invoke the constructor with.</param>
    /// <returns>The result</returns>
    AllocationResult Allocate(CilExecutionContext context, IMethodDescriptor ctor, IList<BitVector> arguments);
}