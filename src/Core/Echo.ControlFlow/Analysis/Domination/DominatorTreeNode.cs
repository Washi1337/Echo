using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;

namespace Echo.ControlFlow.Analysis.Domination
{
    /// <summary>
    /// Represents a single node in a dominator tree of a graph. 
    /// </summary>
    /// <remarks>
    /// It stores the original control flow graph node from which this tree node was inferred, as well a reference to its
    /// immediate dominator, and the node it immediate dominates.
    /// </remarks>
    public class DominatorTreeNode<T> : TreeNodeBase, IIdentifiedNode
    {
        internal DominatorTreeNode(ControlFlowNode<T> node)
        {
            OriginalNode = node;
            Children = new TreeNodeCollection<DominatorTreeNode<T>, DominatorTreeNode<T>>(this);
        }

        /// <inheritdoc/>
        public long Id => OriginalNode.Id;

        /// <summary>
        /// Gets the node that this tree node was derived from. 
        /// </summary>
        public IIdentifiedNode OriginalNode
        {
            get;
        }

        /// <summary>
        /// Gets the children of the current node.
        /// </summary>
        public IList<DominatorTreeNode<T>> Children
        {
            get;
        }

        /// <inheritdoc />
        public override IEnumerable<TreeNodeBase> GetChildren() => Children;

        /// <summary>
        /// Gets a collection of children representing all nodes that were dominated by the original node, as well as an
        /// immediate successor of the original node.
        /// </summary>
        /// <returns>The children, represented by the dominator tree nodes.</returns>
        public IEnumerable<DominatorTreeNode<T>> GetDirectChildren() =>
            Children.Where(c => c.Parent.HasPredecessor(OriginalNode));

        /// <summary>
        /// Gets a collection of children representing all nodes that were dominated by the original node, but were not
        /// an immediate successor of the original node.
        /// </summary>
        /// <returns>The children, represented by the dominator tree nodes.</returns>
        public IEnumerable<DominatorTreeNode<T>> GetIndirectChildren() =>
            Children.Where(c => !c.Parent.HasPredecessor(OriginalNode));

        /// <summary>
        /// Gets all the nodes that are dominated by this control flow graph node.
        /// </summary>
        /// <returns>The nodes that are dominated by this node.</returns>
        /// <remarks>
        /// Because of the nature of dominator analysis, this also includes the current node.
        /// </remarks>
        public IEnumerable<DominatorTreeNode<T>> GetDominatedNodes()
        {
            var stack = new Stack<DominatorTreeNode<T>>();
            stack.Push(this);
            
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                foreach (var child in current.Children)
                    stack.Push(child);
            }
        }
    }
}