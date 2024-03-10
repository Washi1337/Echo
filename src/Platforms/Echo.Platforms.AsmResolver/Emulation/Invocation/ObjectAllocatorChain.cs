using System.Collections.Generic;
using System.Diagnostics;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;

namespace Echo.Platforms.AsmResolver.Emulation.Invocation
{
    /// <summary>
    /// Represents a chain of object allocators that are invoked in sequence until one of the allocators produces a
    /// conclusive result.
    /// </summary>
    [DebuggerDisplay("Count = {Allocators.Count}")]
    public class ObjectAllocatorChain : IObjectAllocator
    {
        /// <summary>
        /// Gets the list of invokers to be called.
        /// </summary>
        public IList<IObjectAllocator> Allocators
        {
            get;
        } = new List<IObjectAllocator>();

        /// <inheritdoc />
        public AllocationResult Allocate(CilExecutionContext context, IMethodDescriptor ctor,
            IList<BitVector> arguments)
        {
            for (int i = 0; i < Allocators.Count; i++)
            {
                var result = Allocators[i].Allocate(context, ctor, arguments);
                if (!result.IsInconclusive)
                    return result;
            }

            return AllocationResult.Inconclusive();
        }
    }
}