using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Regions;
using Echo.Core.Graphing.Analysis.Sorting;

namespace Echo.ControlFlow.Serialization.Blocks
{
    /// <summary>
    /// Provides a mechanism for ordering nodes in control flow graph, based on the outgoing edges of every node. 
    /// </summary>
    public static class BlockSorter
    {
        /// <summary>
        /// Determines an ordering of nodes in the control flow graph in such a way that the basic blocks can be
        /// concatenated together in sequence, and still result in a valid execution of the original program. 
        /// </summary>
        /// <param name="cfg">The control flow graph to pull the nodes from.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the graph.</typeparam>
        /// <returns>The ordering.</returns>
        public static IEnumerable<ControlFlowNode<TInstruction>> SortNodes<TInstruction>(this ControlFlowGraph<TInstruction> cfg)
        {
            // Obtain paths that cannot be reordered.
            var fallThroughPaths = GetFallThroughPaths(cfg);
            
            // Map nodes to the path they are part of.
            var nodeToPath = new Dictionary<ControlFlowNode<TInstruction>, List<ControlFlowNode<TInstruction>>>();
            foreach (var path in fallThroughPaths)
            {
                foreach (var item in path)
                    nodeToPath.Add(item, path);
            }
            
            // Topological sort the unbreakable paths.
            var sorter = new TopologicalSorter<ControlFlowNode<TInstruction>>(GetPathChildren)
            {
                IgnoreCycles = true
            };

            return sorter
                .GetTopologicalSorting(cfg.Entrypoint)
                .Reverse()
                .SelectMany(n => nodeToPath[n]);
            
            IReadOnlyList<ControlFlowNode<TInstruction>> GetPathChildren(ControlFlowNode<TInstruction> node)
            {
                var result = new List<ControlFlowNode<TInstruction>>();
                var path = nodeToPath[node];

                // Get every outgoing edge in the entire path. 
                foreach (var n in path)
                {
                    // Add explicit path successors.
                    if (n.UnconditionalEdge != null && n.UnconditionalEdge.Type == ControlFlowEdgeType.Unconditional)
                        result.Add(nodeToPath[n.UnconditionalNeighbour][0]);
                    AddAdjacencyListToResult(result, n.ConditionalEdges);
                    AddAdjacencyListToResult(result, n.AbnormalEdges);
                    
                    // Check if any exception handler might catch an error within this node.
                    var ehRegion = n.GetParentExceptionHandler();
                    while (ehRegion is {})
                    {
                        if (n.IsInRegion(ehRegion.ProtectedRegion))
                        {
                            foreach (var handlerRegion in ehRegion.HandlerRegions)
                            {
                                var entrypoint = handlerRegion.GetEntrypoint();
                                if (!result.Contains(entrypoint))
                                    result.Add(entrypoint);
                            }
                        }
                
                        ehRegion = ehRegion.GetParentExceptionHandler();
                    }
                }

                return result;
            }

            void AddAdjacencyListToResult(
                IList<ControlFlowNode<TInstruction>> result,
                AdjacencyCollection<TInstruction> adjacency)
            {
                foreach (var edge in adjacency)
                {
                    var target = nodeToPath[edge.Target][0];
                    if (!result.Contains(target))
                        result.Add(target);
                }
            }
        }

        private static List<List<ControlFlowNode<TInstruction>>> GetFallThroughPaths<TInstruction>(
            ControlFlowGraph<TInstruction> cfg)
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

        private static List<ControlFlowNode<TInstruction>> GetFallThroughPath<TInstruction>(
            ControlFlowNode<TInstruction> start, 
            ISet<ControlFlowNode<TInstruction>> visited)
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

        private static ControlFlowNode<TInstruction> GetFallThroughPredecessor<TInstruction>(
            ControlFlowNode<TInstruction> node)
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