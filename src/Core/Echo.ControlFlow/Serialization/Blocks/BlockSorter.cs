using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Serialization.Blocks
{
    public sealed class BlockSorter<TInstruction>
    {
        public IEnumerable<ControlFlowNode<TInstruction>> GetSorting(ControlFlowGraph<TInstruction> cfg)
        {
            var components = GetFallThroughComponents(cfg);
            return components.SelectMany(n => n);
        }
        
        public List<List<ControlFlowNode<TInstruction>>> GetFallThroughComponents(ControlFlowGraph<TInstruction> cfg)
        {
            var visited = new HashSet<ControlFlowNode<TInstruction>>();
            var result = new List<List<ControlFlowNode<TInstruction>>>();
            
            foreach (var node in cfg.Nodes)
            {
                if (visited.Add(node))
                    result.Add(GetFallThroughComponent(node, visited));
            }
;
            return result;
        }

        private List<ControlFlowNode<TInstruction>> GetFallThroughComponent(
            ControlFlowNode<TInstruction> start, ISet<ControlFlowNode<TInstruction>> visited)
        {
            var result = new List<ControlFlowNode<TInstruction>>();
            
            var agenda = new Stack<ControlFlowNode<TInstruction>>();
            agenda.Push(start);
            while (agenda.Count > 0)
            {
                var current = agenda.Pop();
                result.Add(current);

                // Navigate forwards.
                if (current.UnconditionalEdge != null
                    && current.UnconditionalEdge.Type == ControlFlowEdgeType.FallThrough
                    && visited.Add(current.UnconditionalNeighbour))
                {
                    agenda.Push(current.UnconditionalNeighbour);
                }

                // Navigate backwards when necessary.
                int fallthroughPredecessorCount = 0;
                foreach (var incomingEdge in current.GetIncomingEdges())
                {
                    if (incomingEdge.Type == ControlFlowEdgeType.FallThrough)
                    {
                        fallthroughPredecessorCount++;
                        
                        // There can only be one incoming fallthrough edge for every node. If more than one exists,
                        // the input control flow graph is constructed incorrectly.
                        //
                        // Proof: Suppose there exist two distinct basic blocks v, w that share fallthrough neighbour f,
                        // then the footers of both v and w must be able to transfer control to the first instruction
                        // of f without the means of a jump instruction. This can only happen when both footer
                        // instructions of v and w are placed right before the header of f. Therefore, the footers of
                        // v and w must be the same instruction, implying v = w, which is a contradiction.
                        
                        if (fallthroughPredecessorCount > 1)
                        {
                            throw new ArgumentException(
                                $"Node {current.Offset:X8} has multiple fallthrough predecessors.");
                        }

                        var origin = incomingEdge.Origin;
                        if (visited.Add(origin))
                            agenda.Push(origin);
                    }
                }
            }

            return result;
        }
        
    }
}