using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Analysis.Domination;
using Echo.ControlFlow.Specialized;
using Echo.ControlFlow.Specialized.Blocks;

namespace Echo.ControlFlow.Serialization.StructuredFlow
{
    /// <summary>
    /// Provides a default implementation for generating structured program blocks from a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that the control flow graph stores.</typeparam>
    public class StructuredFlowGenerator<TInstruction> : IStructuredFlowGenerator<TInstruction>
    {
        private sealed class DominatorNodeComparer : IComparer<DominatorTreeNode>
        {
            public DominatorTreeNode CurrentParent
            {
                get;
                set;
            }
            
            public int Compare(DominatorTreeNode x, DominatorTreeNode y)
            {
                var node1 = x.OriginalNode;
                var node2 = y.OriginalNode;

                if (node1 == node2)
                    return 0;
                
                // Prioritize nodes that are not terminator nodes.
                if (!node1.GetOutgoingEdges().Any())
                    return !node2.GetOutgoingEdges().Any() ? 0 : 1;
                if (!node2.GetOutgoingEdges().Any())
                    return !node1.GetOutgoingEdges().Any() ? 0 : -1;
                
                // Prioritize fallthrough edges.
                var fallThroughEdge = CurrentParent.OriginalNode.GetFallThroughEdge();
                if (fallThroughEdge.Target == node1)
                    return -1;
                if (fallThroughEdge.Target == node2)
                    return 1;
                
                return 0;
            }
        }

        /// <inheritdoc />
        public ScopeBlock<TInstruction> Generate(ControlFlowGraph<TInstruction> cfg)
        {
            var dominatorTree = DominatorTree.FromGraph(cfg);
            var comparer =new DominatorNodeComparer();
            
            var scopes = new Stack<ScopeBlock<TInstruction>>();
            scopes.Push(new ScopeBlock<TInstruction>());

            var agenda = new Stack<DominatorTreeNode>();
            agenda.Push(dominatorTree.Root);

            while (agenda.Count > 0)
            {
                var current = agenda.Pop();
                var currentNode = (Node<BasicBlock<TInstruction>>) current.OriginalNode;
                
                // Add current node to the current block.
                scopes.Peek().Blocks.Add(currentNode.Contents);

                // Obtain sorted list of children.
                comparer.CurrentParent = current;
                var children = GetSortedChildren(current, comparer);

                // Schedule them for processing (= push them in reverse order).
                foreach (var child in children.Reverse())
                    agenda.Push(child);
            }
                
            return scopes.Pop();
        }

        private static IList<DominatorTreeNode> GetSortedChildren(DominatorTreeNode node, IComparer<DominatorTreeNode> comparer)
        {
            var result = new List<DominatorTreeNode>();

            var directChildren = node
                .GetDirectChildren()
                .OrderBy(x => x, comparer)
#if DEBUG
                .ToArray();
#endif
            var indirectChildren = node
                .GetIndirectChildren()
                .OrderBy(x => x, comparer)
#if DEBUG
                .ToArray();
#endif
            
            result.AddRange(directChildren);
            result.AddRange(indirectChildren);
            
            
            return result;
        }
        
        

    }
}