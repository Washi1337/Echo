using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.ControlFlow.Regions.Detection
{
    /// <summary>
    /// Provides methods for detecting exception handler regions in a control flow graph by providing address ranges
    /// indicating the protected and handler regions.
    /// </summary>
    public static class RangedEHRegionDetector
    {
        /// <summary>
        /// Creates new exception handler regions in the provided control flow graph, based on a collection of address
        /// ranges indicating exception handlers.
        /// </summary>
        /// <param name="cfg">The control flow graph to create regions in.</param>
        /// <param name="ranges">The exception handler address ranges.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
        public static void DetectExceptionHandlerRegions<TInstruction>(
            this ControlFlowGraph<TInstruction> cfg,
            IEnumerable<ExceptionHandlerRange> ranges)
        {
            // Sort all ranges by their start and end offsets.
            var sortedRanges = ranges.ToList();
            sortedRanges.Sort();

            var rangeToRegionMapping = CreateEHRegions(cfg, sortedRanges);
            InsertNodesInEHRegions(cfg, sortedRanges, rangeToRegionMapping);
        }

        private static Dictionary<AddressRange, BasicControlFlowRegion<TInstruction>> CreateEHRegions<TInstruction>(
            ControlFlowGraph<TInstruction> cfg, 
            IReadOnlyList<ExceptionHandlerRange> sortedRanges)
        {
            var rangeToRegion = new Dictionary<AddressRange, BasicControlFlowRegion<TInstruction>>();

            var ehRegions = new Dictionary<AddressRange, ExceptionHandlerRegion<TInstruction>>();
            for (int i = 0; i < sortedRanges.Count; i++)
            {
                var currentEHRange = sortedRanges[i];
                if (!ehRegions.TryGetValue(currentEHRange.ProtectedRange, out var ehRegion))
                {
                    // Register new EH region for the protected range.
                    ehRegion = new ExceptionHandlerRegion<TInstruction>();
                    ehRegions.Add(currentEHRange.ProtectedRange, ehRegion);
                    rangeToRegion.Add(currentEHRange.ProtectedRange, ehRegion.ProtectedRegion);

                    // Since the ranges are sorted by enclosing EHs first, we can backtrack the list of ranges to find.
                    // the parent region (if there is any).
                    BasicControlFlowRegion<TInstruction> parentRegion = null;
                    for (int j = i; j >= 0 && parentRegion is null; j--)
                    {
                        var potentialParentRange = sortedRanges[j];
                        if (potentialParentRange.ProtectedRange.Contains(currentEHRange.ProtectedRange))
                            parentRegion = rangeToRegion[potentialParentRange.ProtectedRange];
                        if (potentialParentRange.HandlerRange.Contains(currentEHRange.HandlerRange))
                            parentRegion = rangeToRegion[potentialParentRange.HandlerRange];
                    }

                    // Insert region into graph or parent region.
                    if (parentRegion is null)
                        cfg.Regions.Add(ehRegion);
                    else
                        parentRegion.Regions.Add(ehRegion);
                }

                // Register handler region.
                var handlerRegion = new BasicControlFlowRegion<TInstruction>();
                ehRegion.HandlerRegions.Add(handlerRegion);
                rangeToRegion.Add(currentEHRange.HandlerRange, handlerRegion);
            }

            return rangeToRegion;
        }

        private static void InsertNodesInEHRegions<TInstruction>(
            ControlFlowGraph<TInstruction> cfg, 
            IReadOnlyList<ExceptionHandlerRange> sortedRanges,
            Dictionary<AddressRange, BasicControlFlowRegion<TInstruction>> rangeToRegionMapping)
        {
            foreach (var node in cfg.Nodes)
            {
                var parentRange = GetParentRange(sortedRanges, node);
                if (parentRange.HasValue)
                    rangeToRegionMapping[parentRange.Value].Nodes.Add(node);
            }
        }

        private static AddressRange? GetParentRange<TInstruction>(
            IReadOnlyList<ExceptionHandlerRange> sortedRanges, 
            ControlFlowNode<TInstruction> node)
        {
            // Since the ranges are sorted in such a way that outer ranges are coming first, we can simply reverse the
            // linear lookup to get the smallest EH region that this node contains. 
            
            for (int i = sortedRanges.Count - 1; i >= 0; i--)
            {
                var currentRange = sortedRanges[i];

                if (currentRange.ProtectedRange.Contains(node.Offset))
                    return currentRange.ProtectedRange;
                if (currentRange.HandlerRange.Contains(node.Offset))
                    return currentRange.HandlerRange;
            }

            return null;
        }
    }
}