using System.Collections.Generic;
using System.Linq;
using Echo.Graphing.Analysis.Sorting;

namespace Echo.ControlFlow.Serialization.Blocks
{
    /// <summary>
    /// Provides a mechanism for ordering nodes in control flow graph, based on the outgoing edges of every node. 
    /// </summary>
    public static class BlockSorter
    {
        // -------------------------
        // Implementation rationale
        // -------------------------
        //
        // Two key observations are:
        //
        // - Topological orderings of nodes for directed acyclic graphs (DAGs) are orderings that "respect" dominance
        //   without actually needing to compute the entire dominator tree. Nodes sorted by dominance are easier 
        //   to read, since they resemble structured flow more. If node A dominates B, it would mean that in the
        //   resulting ordering, node A will appear before node B, as it would appear in "normal" programs.
        //   For cyclic graphs we can simply ignore back-edges to turn the input graph into a DAG.
        //
        // - Paths constructed by fallthrough edges in the control flow graph cannot be broken up into smaller
        //   paths in the resulting node sequence without changing the code of the basic block (e.g. inserting a goto).
        //   This puts constraints on what kind of topological orderings we can construct.
        //
        // In this implementation, we create a "view" on the input control flow graph, that contracts nodes in a single
        // path induced by fallthrough edges into a single node. It is safe to put the nodes of this new graph in any
        // ordering without invalidating the requirements for fallthrough edges, since the new nodes never have outgoing
        // fallthrough edges by construction. The exception to this rule is the entry point node, which always has to
        // start at the beginning of the sequence. Therefore, doing a topological sorting starting at this entry point
        // node results in a valid sequence of basic blocks that is reasonably readable.
        //
        // There is still some form of "non-determinism" in the algorithm, as neighbours of each node are still somewhat
        // arbitrarily ordered.  As a result, an exit point block (e.g. a block with a return) might still end up
        // somewhere in the middle of the sequence, which can be somewhat counter-intuitive.
        
        /// <summary>
        /// Determines an ordering of nodes in the control flow graph in such a way that the basic blocks can be
        /// concatenated together in sequence, and still result in a valid execution of the original program. 
        /// </summary>
        /// <param name="cfg">The control flow graph to pull the nodes from.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the graph.</typeparam>
        /// <returns>The ordering.</returns>
        public static IEnumerable<ControlFlowNode<TInstruction>> SortNodes<TInstruction>(
            this ControlFlowGraph<TInstruction> cfg)
        {
            var pathsView = DetermineUnbreakablePaths(cfg);
            var sorter = new TopologicalSorter<ControlFlowNode<TInstruction>>(pathsView.GetImpliedNeighbours, true);

            return sorter
                .GetTopologicalSorting(cfg.EntryPoint)
                .Reverse()
                .SelectMany(n => pathsView.GetUnbreakablePath(n));
        }

        private static UnbreakablePathsView<TInstruction> DetermineUnbreakablePaths<TInstruction>(
            ControlFlowGraph<TInstruction> cfg)
        {
            var visited = new HashSet<ControlFlowNode<TInstruction>>();
            var result = new UnbreakablePathsView<TInstruction>();
            
            foreach (var node in cfg.Nodes)
            {
                if (!visited.Contains(node))
                    result.AddUnbreakablePath(GetFallThroughPath(node, visited));
            }

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