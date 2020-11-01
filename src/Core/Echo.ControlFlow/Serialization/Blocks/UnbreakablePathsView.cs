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
                if (ehRegion.PrologueRegion is {} && node.IsInRegion(ehRegion.PrologueRegion))
                {
                    // If there is a prologue region, the handlers will be the successors.
                    // This is not explicitly specified as an edge in the CFG.
                    AddExceptionHandlerSuccessors(result, ehRegion);
                }
                else if (node.IsInRegion(ehRegion.ProtectedRegion))
                {
                    // If the EH region has a prologue region, the predecessor of the handler
                    // regions will be the prologue region. Otherwise the predecessor will be the
                    // protected region. This is not explicitly specified as an edge in the CFG.
                    if (ehRegion.PrologueRegion is {})
                        AddSuccessorToResult(result, GetRegionEntryPoint(ehRegion.PrologueRegion, "Prologue region"));
                    else
                        AddExceptionHandlerSuccessors(result, ehRegion);
                }
                else if (ehRegion.EpilogueRegion is {})
                {
                    // If there is an epilogue region, the handlers' successors will be the epilogue region.
                    // This is not explicitly specified as an edge in the CFG.
                    foreach (var handler in ehRegion.HandlerRegions)
                    {
                        if (!node.IsInRegion(handler))
                            continue;

                        AddSuccessorToResult(result, GetRegionEntryPoint(ehRegion.EpilogueRegion, "Epilogue region"));
                    }
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