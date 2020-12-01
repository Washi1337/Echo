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

        public void AddUnbreakablePath(IList<ControlFlowNode<TInstruction>> path)
        {
            foreach (var item in path)
                _nodeToPath.Add(item, path);
        }

        public IList<ControlFlowNode<TInstruction>> GetUnbreakablePath(ControlFlowNode<TInstruction> node)
        {
            return _nodeToPath[node];
        }

        public IReadOnlyList<ControlFlowNode<TInstruction>> GetImpliedNeighbours(ControlFlowNode<TInstruction> node)
        {
            var result = new List<ControlFlowNode<TInstruction>>();
            var path = GetUnbreakablePath(node);

            // Get every outgoing edge in the entire path. 
            foreach (var n in path)
            {
                // Add unconditional edge.
                if (n.UnconditionalEdge != null && n.UnconditionalEdge.Type == ControlFlowEdgeType.Unconditional)
                    AddSuccessorToResult(result, n.UnconditionalNeighbour);

                // Add explicit conditional / abnormal edges.
                AddAdjacencyListToResult(result, n.ConditionalEdges);
                AddAdjacencyListToResult(result, n.AbnormalEdges);

                // Check if any exception handler might catch an error within this node.
                AddPotentialHandlerSuccessors(result, n);
            }

            return result;
        }

        private void AddSuccessorToResult(
            ICollection<ControlFlowNode<TInstruction>> result,
            ControlFlowNode<TInstruction> node)
        {
            var target = GetUnbreakablePath(node)[0];
            if (!result.Contains(target))
                result.Add(target);
        }

        private void AddAdjacencyListToResult(
            ICollection<ControlFlowNode<TInstruction>> result,
            AdjacencyCollection<TInstruction> adjacency)
        {
            foreach (var edge in adjacency)
                AddSuccessorToResult(result, edge.Target);
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
                        var handlerEntry = handlerRegion.GetEntrypoint();
                        if (handlerEntry is null)
                        {
                            throw new InvalidOperationException(
                                $"Handler region {i} of exception handler does not have an entrypoint assigned.");
                        }

                        AddSuccessorToResult(result, handlerEntry);
                    }
                }
                else
                {
                    var handlerRegion = node.GetParentHandler();

                    ControlFlowNode<TInstruction> nextEntry = null;
                    if (node.ParentRegion == handlerRegion.Prologue)
                        nextEntry = handlerRegion.Contents.Entrypoint;
                    if (nextEntry is null && node.ParentRegion == handlerRegion.Contents)
                        nextEntry = handlerRegion.Epilogue?.Entrypoint;

                    if (nextEntry != null)
                        AddSuccessorToResult(result, nextEntry);
                }

                // Move up the EH region tree.
                ehRegion = ehRegion.GetParentExceptionHandler();
            }
        }

        private void AddExceptionHandlerSuccessors(
            ICollection<ControlFlowNode<TInstruction>> result,
            ExceptionHandlerRegion<TInstruction> ehRegion)
        {
            for (int i = 0; i < ehRegion.HandlerRegions.Count; i++)
            {
                var handlerRegion = ehRegion.HandlerRegions[i];

                // Ensure the handler that we can jump to has an entrypoint node.
                var entry = GetRegionEntryPoint(
                    handlerRegion, $"Handler region {i} of exception handler");
                AddSuccessorToResult(result, entry);
            }
        }

        private static ControlFlowNode<TInstruction> GetRegionEntryPoint(
            IControlFlowRegion<TInstruction> region,
            string name)
        {
            var entry = region.GetEntrypoint();
            if (entry is null)
                throw new InvalidOperationException(name + " does not have an entrypoint assigned.");

            return entry;
        }
    }
}