using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Regions;

namespace Echo.ControlFlow.Serialization.Blocks
{
    internal sealed class UnbreakablePathsView<TInstruction>
        where TInstruction : notnull
    {
        private readonly Dictionary<ControlFlowNode<TInstruction>, IList<ControlFlowNode<TInstruction>>> _nodeToPath = new();
        private readonly Dictionary<ControlFlowRegion<TInstruction>, IList<ControlFlowNode<TInstruction>>> _regionSuccessors = new();

        public void AddUnbreakablePath(IList<ControlFlowNode<TInstruction>> path)
        {
            for (int i = 0; i < path.Count; i++)
                _nodeToPath.Add(path[i], path);
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
            for (int i = 0; i < path.Count; i++)
            {
                var n = path[i];
                
                // Add unconditional edge.
                if (n.UnconditionalEdge is {Type: ControlFlowEdgeType.Unconditional, Target: { } neighbour})
                    AddSuccessorToResult(result, neighbour);

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
            // If the node is in an exception handler, there are a couple "implicit" successors.
            //
            // - Any node in the protected region has an implicit successor to the start of every handler region.
            //
            // - Any node in a prologue/body/epilogue of a handler region has an implicit successor of the next 
            //   embedded region. That is, a prologue node can transfer control to the body, and body to epilogue.
            //
            // - Any node in a handler region also implicitly can transfer control to one of the successors of any
            //   node in the protected region (e.g. a try-finally construct does not have explicit outgoing branches).
            //
            // These successors are not explicitly encoded as an edge in the input CFG, but do often matter
            // when it comes to ordering these nodes (e.g. CIL requires handlers to be after the protected region).
            // Therefore, we add these as "virtual" successors to the list, to ensure the topological sorting
            // takes this into account.
            
            var ehRegion = node.GetParentExceptionHandler();

            while (ehRegion is not null)
            {
                // If we entered this loop, it means the node is either in the protected region or a handler region
                // of an exception handler.
                if (node.IsInRegion(ehRegion.ProtectedRegion))
                {
                    AddHandlerEntryPoints(result, ehRegion);
                }
                else
                {
                    AddNextHandlerRegion(result, node);
                    AddRegionSuccessors(result, ehRegion.ProtectedRegion);
                }

                // Move up the EH region tree.
                ehRegion = ehRegion.GetParentExceptionHandler();
            }
        }

        private void AddHandlerEntryPoints(
            ICollection<ControlFlowNode<TInstruction>> result, 
            ExceptionHandlerRegion<TInstruction> ehRegion)
        {
            for (int i = 0; i < ehRegion.Handlers.Count; i++)
            {
                var handlerRegion = ehRegion.Handlers[i];

                // Ensure the handler that we can jump to has an entrypoint node.
                var handlerEntry = handlerRegion.GetEntryPoint();
                if (handlerEntry is null)
                {
                    throw new InvalidOperationException(
                        $"Handler region {i.ToString()} of exception handler does not have an entrypoint assigned.");
                }

                AddSuccessorToResult(result, handlerEntry);
            }
        }

        private void AddNextHandlerRegion(ICollection<ControlFlowNode<TInstruction>> result, ControlFlowNode<TInstruction> node)
        {
            var handlerRegion = node.GetParentHandler()!;
            
            ControlFlowNode<TInstruction>? nextEntry = null;
            if (node.ParentRegion == handlerRegion.Prologue)
                nextEntry = handlerRegion.Contents.EntryPoint;
            if (nextEntry is null && node.ParentRegion == handlerRegion.Contents)
                nextEntry = handlerRegion.Epilogue?.EntryPoint;

            if (nextEntry != null)
                AddSuccessorToResult(result, nextEntry);
        }

        private void AddRegionSuccessors(ICollection<ControlFlowNode<TInstruction>> result, ControlFlowRegion<TInstruction> region)
        {
            if (!_regionSuccessors.TryGetValue(region, out var successors))
            {
                successors = region.GetSuccessors().ToArray();
                _regionSuccessors.Add(region, successors);
            }

            for (int i = 0; i < successors.Count; i++)
                AddSuccessorToResult(result, successors[i]);
        }
    }
}