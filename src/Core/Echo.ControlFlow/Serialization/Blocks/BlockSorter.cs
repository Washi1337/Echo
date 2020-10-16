using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Serialization.Blocks
{
    public sealed class BlockSorter<TInstruction>
    {
        public IEnumerable<ControlFlowNode<TInstruction>> GetSorting(ControlFlowGraph<TInstruction> cfg)
        {
            var fallThroughPaths = GetFallThroughPaths(cfg);
            return fallThroughPaths.SelectMany(n => n);
        }
        
        public List<List<ControlFlowNode<TInstruction>>> GetFallThroughPaths(ControlFlowGraph<TInstruction> cfg)
        {
            var visited = new HashSet<ControlFlowNode<TInstruction>>();
            var result = new List<List<ControlFlowNode<TInstruction>>>();
            
            foreach (var node in cfg.Nodes)
            {
                if (!visited.Contains(node))
                    result.Add(GetFallThroughPath(node, visited));
            }
;
            return result;
        }

        private List<ControlFlowNode<TInstruction>> GetFallThroughPath(
            ControlFlowNode<TInstruction> start, ISet<ControlFlowNode<TInstruction>> visited)
        {
            // Navigate back to root of fallthrough path.
            var predecessor = start;
            do
            {
                start = predecessor;
                predecessor = GetFallThroughPredecessor(start);
            } while (predecessor != null);

            var result = new List<ControlFlowNode<TInstruction>>();

            var agenda = new Stack<ControlFlowNode<TInstruction>>();
            agenda.Push(start);
            while (agenda.Count > 0)
            {
                var current = agenda.Pop();
                if (!visited.Add(current))
                    continue;
                
                result.Add(current);

                // Navigate forwards.
                if (current.UnconditionalEdge != null
                    && current.UnconditionalEdge.Type == ControlFlowEdgeType.FallThrough)
                {
                    agenda.Push(current.UnconditionalNeighbour);
                }

                // Verify that the current node has only one fallthrough predecessor.
                GetFallThroughPredecessor(current);
            }
            
            return result;
        }

        private static ControlFlowNode<TInstruction> GetFallThroughPredecessor(ControlFlowNode<TInstruction> node)
        {
            // There can only be one incoming fallthrough edge for every node. If more than one exists,
            // the input control flow graph is constructed incorrectly.
            //
            // Proof: Suppose there exist two distinct basic blocks v, w that share fallthrough neighbour f,
            // then the footers of both v and w must be able to transfer control to the first instruction
            // of f without the means of a jump instruction. This can only happen when both footer
            // instructions of v and w are placed right before the header of f. Therefore, the footers of
            // v and w must be the same instruction, implying v = w, which is a contradiction.

            ControlFlowNode<TInstruction> predecessor = null;
            
            foreach (var incomingEdge in node.GetIncomingEdges())
            {
                if (incomingEdge.Type == ControlFlowEdgeType.FallThrough)
                {
                    if (predecessor != null)
                    {
                        throw new BlockOrderingException(
                            $"Node {node.Offset:X8} has multiple fallthrough predecessors.");
                    }

                    predecessor = incomingEdge.Origin;
                }
            }

            return predecessor;
        }
    }
}