using System.Collections.Generic;
using System.Linq;

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
            where TInstruction : notnull
        {
            // Sort all ranges by their start and end offsets.
            var sortedRanges = ranges.ToList();
            sortedRanges.Sort();

            // Build up region tree.
            var rangeToRegionMapping = CreateEHRegions(cfg, sortedRanges);
            InsertNodesInEHRegions(cfg, sortedRanges, rangeToRegionMapping);
            DetermineRegionEntrypoints(cfg, sortedRanges, rangeToRegionMapping);
        }

        private static Dictionary<AddressRange, ScopeRegion<TInstruction>> CreateEHRegions<TInstruction>(
            ControlFlowGraph<TInstruction> cfg, 
            IReadOnlyList<ExceptionHandlerRange> sortedRanges)
            where TInstruction : notnull
        {
            var result = new Dictionary<AddressRange, ScopeRegion<TInstruction>>();

            var ehRegions = new Dictionary<AddressRange, ExceptionHandlerRegion<TInstruction>>();
            for (int i = 0; i < sortedRanges.Count; i++)
            {
                var currentEHRange = sortedRanges[i];
                
                // We want to merge exception handlers that have the exact same protected region.
                // This allows for exception handler constructs that have multiple handler blocks.
                
                // Check if we have already created the EH region in an earlier iteration: 
                if (!ehRegions.TryGetValue(currentEHRange.ProtectedRange, out var ehRegion))
                {
                    // If not, create and register a new EH region for the protected range.
                    ehRegion = new ExceptionHandlerRegion<TInstruction>();
                    ehRegions.Add(currentEHRange.ProtectedRange, ehRegion);
                    result.Add(currentEHRange.ProtectedRange, ehRegion.ProtectedRegion);
                    
                    // We need to add the EH region to a parent region. This can either be the CFG itself, or a
                    // sub region that was previously added. 
                    var parentRegion = FindParentRegion(result, sortedRanges, i);
                    if (parentRegion is null)
                        cfg.Regions.Add(ehRegion);
                    else
                        parentRegion.Regions.Add(ehRegion);
                }

                // Create and add new handler region from the range.
                var handlerRegion = new HandlerRegion<TInstruction>();
                handlerRegion.Tag = currentEHRange.UserData;
                ehRegion.Handlers.Add(handlerRegion);
                result.Add(currentEHRange.HandlerRange, handlerRegion.Contents);
                
                // Do we need to add a prologue block?
                if (currentEHRange.PrologueRange != AddressRange.NilRange)
                {
                    handlerRegion.Prologue = new ScopeRegion<TInstruction>();
                    ehRegions.Add(currentEHRange.PrologueRange, ehRegion);
                    result.Add(currentEHRange.PrologueRange, handlerRegion.Prologue);
                }

                // Do we need to add an epilogue block?
                if (currentEHRange.EpilogueRange != AddressRange.NilRange)
                {
                    handlerRegion.Epilogue = new ScopeRegion<TInstruction>();
                    ehRegions.Add(currentEHRange.EpilogueRange, ehRegion);
                    result.Add(currentEHRange.EpilogueRange, handlerRegion.Epilogue);
                }
            }

            return result;
        }

        private static ScopeRegion<TInstruction>? FindParentRegion<TInstruction>(
            Dictionary<AddressRange, ScopeRegion<TInstruction>> regions,
            IReadOnlyList<ExceptionHandlerRange> sortedRanges,
            int currentRangeIndex)
            where TInstruction : notnull
        {
            var ehRange = sortedRanges[currentRangeIndex];
            
            // Since the ranges are sorted by enclosing EHs first, we can backtrack the list of ranges to find.
            // the parent region (if there is any).
            
            for (int j = currentRangeIndex; j >= 0; j--)
            {
                var potentialParentRange = sortedRanges[j];
                if (potentialParentRange.ProtectedRange.Contains(ehRange.ProtectedRange))
                    return regions[potentialParentRange.ProtectedRange];
                if (potentialParentRange.PrologueRange.Contains(ehRange.PrologueRange))
                    return regions[potentialParentRange.PrologueRange];
                if (potentialParentRange.HandlerRange.Contains(ehRange.HandlerRange))
                    return regions[potentialParentRange.HandlerRange];
                if (potentialParentRange.EpilogueRange.Contains(ehRange.EpilogueRange))
                    return regions[potentialParentRange.EpilogueRange];
            }

            return null;
        }

        private static void InsertNodesInEHRegions<TInstruction>(
            ControlFlowGraph<TInstruction> cfg, 
            IReadOnlyList<ExceptionHandlerRange> sortedRanges,
            Dictionary<AddressRange, ScopeRegion<TInstruction>> rangeToRegionMapping)
            where TInstruction : notnull
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
            where TInstruction : notnull
        {
            // Since the ranges are sorted in such a way that outer ranges are coming first, we can simply reverse the
            // linear lookup to get the smallest EH region that this node contains. 
            
            for (int i = sortedRanges.Count - 1; i >= 0; i--)
            {
                var currentRange = sortedRanges[i];

                if (currentRange.ProtectedRange.Contains(node.Offset))
                    return currentRange.ProtectedRange;
                if (currentRange.PrologueRange.Contains(node.Offset))
                    return currentRange.PrologueRange;
                if (currentRange.HandlerRange.Contains(node.Offset))
                    return currentRange.HandlerRange;
                if (currentRange.EpilogueRange.Contains(node.Offset))
                    return currentRange.EpilogueRange;
            }

            return null;
        }

        private static void DetermineRegionEntrypoints<TInstruction>(
            ControlFlowGraph<TInstruction> cfg, 
            List<ExceptionHandlerRange> sortedRanges,
            Dictionary<AddressRange, ScopeRegion<TInstruction>> rangeToRegionMapping)
            where TInstruction : notnull
        {
            foreach (var range in sortedRanges)
            {
                var protectedRegion = rangeToRegionMapping[range.ProtectedRange];
                protectedRegion.EntryPoint ??= cfg.Nodes.GetByOffset(range.ProtectedRange.Start);

                var handlerRegion = rangeToRegionMapping[range.HandlerRange];
                handlerRegion.EntryPoint ??= cfg.Nodes.GetByOffset(range.HandlerRange.Start);

                if (rangeToRegionMapping.TryGetValue(range.PrologueRange, out var prologueRegion))
                    prologueRegion.EntryPoint ??= cfg.Nodes.GetByOffset(range.PrologueRange.Start);

                if (rangeToRegionMapping.TryGetValue(range.EpilogueRange, out var epilogueRegion))
                    epilogueRegion.EntryPoint ??= cfg.Nodes.GetByOffset(range.EpilogueRange.Start);
            }
        }
        
    }
}