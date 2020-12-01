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
                // entrypoint of every handler. Also, a successor of a handler prologue is the code
                // of the actual handler block, which also potentially has an implicit successor to the epilogue.
                //
                // These successors are not explicitly encoded as an edge in the input CFG, but do often matter
                // when it comes to ordering these nodes (e.g. CIL requires handlers to be after the protected region).
                // Therefore, we add these as "virtual" successors to the list, to ensure the topological sorting
                // takes this into account.

                // If we entered this loop, it means the node is either in the protected region or a handler region
                // of an exception handler.
                if (node.IsInRegion(ehRegion.ProtectedRegion))
                    AddHandlerEntrypoints(result, ehRegion);
                else
                    AddNextHandlerRegion(result, node, node.GetParentHandler());

                // Move up the EH region tree.
                ehRegion = ehRegion.GetParentExceptionHandler();
            }
        }

        private void AddHandlerEntrypoints(
            ICollection<ControlFlowNode<TInstruction>> result, 
            ExceptionHandlerRegion<TInstruction> ehRegion)
        {
            for (int i = 0; i < ehRegion.Handlers.Count; i++)
            {
                var handlerRegion = ehRegion.Handlers[i];

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

        private void AddNextHandlerRegion(
            ICollection<ControlFlowNode<TInstruction>> result, 
            ControlFlowNode<TInstruction> node, 
            HandlerRegion<TInstruction> handlerRegion)
        {
            ControlFlowNode<TInstruction> nextEntry = null;
            if (node.ParentRegion == handlerRegion.Prologue)
                nextEntry = handlerRegion.Contents.Entrypoint;
            if (nextEntry is null && node.ParentRegion == handlerRegion.Contents)
                nextEntry = handlerRegion.Epilogue?.Entrypoint;

            if (nextEntry != null)
                AddSuccessorToResult(result, nextEntry);
        }

    }
}