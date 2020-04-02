using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Regions.Detection
{
    public static class ExceptionHandlerRegionDetector
    {
        public static void DetectExceptionHandlerRegions<TInstruction>(this ControlFlowGraph<TInstruction> cfg, IEnumerable<ExceptionHandlerRange> ranges)
        {
            var sortedRanges = ranges.ToList();
            sortedRanges.Sort((a, b) =>
            {
                if (a.ProtectedRange.Start > b.ProtectedRange.Start)
                {
                    if (a.ProtectedRange.End < b.ProtectedRange.End)
                        return -1;
                    if (a.ProtectedRange.Start >= b.ProtectedRange.End)
                        return 1;
                    throw new ArgumentException("Overlapping exception handlers.");
                }
                
                if (a.ProtectedRange.Start < b.ProtectedRange.Start)
                {
                    if (a.ProtectedRange.End > b.ProtectedRange.End)
                        return 1;
                    if (a.ProtectedRange.Start <= b.ProtectedRange.End)
                        return -1;
                    throw new ArgumentException("Overlapping exception handlers.");
                }

                if (a.HandlerRange.Start < b.HandlerRange.Start)
                    return -1;
                
                if (a.HandlerRange.Start > b.HandlerRange.Start)
                    return 1;
                
                return 0;
            });
            
            var remainingNodes = new HashSet<ControlFlowNode<TInstruction>>(cfg.Nodes);
            
            foreach (var range in sortedRanges)
            {
                var ehRegion = new ExceptionHandlerRegion<TInstruction>();
                cfg.Regions.Add(ehRegion);
                
                foreach (var node in remainingNodes.ToArray())
                {
                    BasicControlFlowRegion<TInstruction> region = null;
                    if (range.ProtectedRange.Contains(node.Offset))
                        region = ehRegion.ProtectedRegion;
                    else if (range.HandlerRange.Contains(node.Offset))
                        region = ehRegion.HandlerRegion;

                    if (region is {})
                    {
                        region.Nodes.Add(node);
                        remainingNodes.Remove(node);
                    }
                }
            }
            
        }
        
    }
}