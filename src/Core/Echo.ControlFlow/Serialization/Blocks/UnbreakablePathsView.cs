using System;
using System.Collections.Generic;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Regions;

namespace Echo.ControlFlow.Serialization.Blocks
{
    internal sealed class UnbreakablePathsView<TInstruction> 
    { 
        private readonly Dictionary<ControlFlowNode<TInstruction>, IList<ControlFlowNode<TInstruction>>> _nodeToPath =
            new Dictionary<ControlFlowNode<TInstruction>, IList<ControlFlowNode<TInstruction>>>(); 
            
        public void AddPath(IList<ControlFlowNode<TInstruction>> path)
        {
            foreach (var item in path)
                _nodeToPath.Add(item, path);
        }

        public IList<ControlFlowNode<TInstruction>> GetPath(ControlFlowNode<TInstruction> node)
        {
            return _nodeToPath[node];
        }

        public IReadOnlyList<ControlFlowNode<TInstruction>> GetImpliedNeighbours(ControlFlowNode<TInstruction> node)
        {
            var result = new List<ControlFlowNode<TInstruction>>();
            var path = GetPath(node);

            // Get every outgoing edge in the entire path. 
            foreach (var n in path)
            {
                // Add unconditional edge.
                if (n.UnconditionalEdge != null && n.UnconditionalEdge.Type == ControlFlowEdgeType.Unconditional)
                {
                    var neighbourEntry = GetPath(n.UnconditionalNeighbour)[0];
                    if (!result.Contains(neighbourEntry))
                        result.Add(neighbourEntry);
                }

                // Add explicit conditional / abnormal edges.
                AddAdjacencyListToResult(result, n.ConditionalEdges);
                AddAdjacencyListToResult(result, n.AbnormalEdges);
                
                // Check if any exception handler might catch an error within this node.
                AddPotentialHandlerSuccessors(result, n);
            }
            
            return result;
        }

        private void AddAdjacencyListToResult(
            ICollection<ControlFlowNode<TInstruction>> result,
            AdjacencyCollection<TInstruction> adjacency)
        {
            foreach (var edge in adjacency)
            {
                var target = GetPath(edge.Target)[0];
                if (!result.Contains(target))
                    result.Add(target);
            }
        }

        private void AddPotentialHandlerSuccessors(
            ICollection<ControlFlowNode<TInstruction>> result, 
            ControlFlowNode<TInstruction> node)
        {
            var ehRegion = node.GetParentExceptionHandler();
            
            while (ehRegion is {})
            {
                // If the node is in a protected region of an exception handler, a potential successor is the
                // entrypoint of every handler. This is not explicitly specified as an edge in the CFG.
                
                if (node.IsInRegion(ehRegion.ProtectedRegion))
                {
                    for (int i = 0; i < ehRegion.HandlerRegions.Count; i++)
                    {
                        var handlerRegion = ehRegion.HandlerRegions[i];

                        // Ensure the handler that we can jump to has an entrypoint node.
                        var entrypoint = handlerRegion.GetEntrypoint();
                        if (entrypoint is null)
                        {
                            throw new InvalidOperationException(
                                $"Handler region {i} of exception handler does not have an entrypoint assigned.");
                        }

                        var target = GetPath(entrypoint)[0];
                        if (!result.Contains(target))
                            result.Add(target);
                    }
                }

                // Move up the EH region tree.
                ehRegion = ehRegion.GetParentExceptionHandler();
            }
        }
        
    }
}