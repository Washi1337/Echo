using System;
using System.Collections.Generic;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Regions;

namespace Echo.ControlFlow.Serialization.Blocks
{
    internal sealed class UnbreakablePathsView<TInstruction> 
    {
        private readonly List<IList<ControlFlowNode<TInstruction>>> _paths = new List<IList<ControlFlowNode<TInstruction>>>(); 
        private readonly Dictionary<ControlFlowNode<TInstruction>, IList<ControlFlowNode<TInstruction>>> _nodeToPath =
            new Dictionary<ControlFlowNode<TInstruction>, IList<ControlFlowNode<TInstruction>>>(); 
            
        public void AddPath(IList<ControlFlowNode<TInstruction>> path)
        {
            foreach (var item in path)
                _nodeToPath.Add(item, path);
            _paths.Add(path);
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
                // Add explicit path successors.
                if (n.UnconditionalEdge != null && n.UnconditionalEdge.Type == ControlFlowEdgeType.Unconditional)
                {
                    var neighbourEntry = GetPath(n.UnconditionalNeighbour)[0];
                    if (!result.Contains(neighbourEntry))
                        result.Add(neighbourEntry);
                }

                AddAdjacencyListToResult(n.ConditionalEdges, result);
                AddAdjacencyListToResult(n.AbnormalEdges, result);
                
                // Check if any exception handler might catch an error within this node.
                var ehRegion = n.GetParentExceptionHandler();
                while (ehRegion is {})
                {
                    if (n.IsInRegion(ehRegion.ProtectedRegion))
                    {
                        for (int i = 0; i < ehRegion.HandlerRegions.Count; i++)
                        {
                            var handlerRegion = ehRegion.HandlerRegions[i];
                            
                            // Ensure the handler has an entrypoint that we can jump to.
                            var entrypoint = handlerRegion.GetEntrypoint();
                            if (entrypoint is null)
                            {
                                throw new InvalidOperationException(
                                    $"Handler region {i} of exception handler does not have an entrypoint assigned.");
                            }

                            var entryNode = GetPath(entrypoint)[0];
                            if (!result.Contains(entryNode))
                                result.Add(entryNode);
                        }
                    }
                
                    ehRegion = ehRegion.GetParentExceptionHandler();
                }
            }
            
            return result;
        }

        private void AddAdjacencyListToResult(
            AdjacencyCollection<TInstruction> adjacency,
            List<ControlFlowNode<TInstruction>> result)
        {
            foreach (var edge in adjacency)
            {
                var target = GetPath(edge.Target)[0];
                if (!result.Contains(target))
                    result.Add(target);
            }
        }
    }
}