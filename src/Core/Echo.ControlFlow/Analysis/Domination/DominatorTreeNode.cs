using System.Collections.Generic;
using System.Linq;
using Echo.Graphing;

namespace Echo.ControlFlow.Analysis.Domination
{
    /// <summary>
    /// Represents a single node in a dominator tree of a graph. 
    /// </summary>
    /// <remarks>
    /// It stores the original control flow graph node from which this tree node was inferred, as well a reference to its
    /// immediate dominator, and the node it immediate dominates.
    /// </remarks>
    public class DominatorTreeNode<TInstruction> : TreeNodeBase
        where TInstruction : notnull
    {
        internal DominatorTreeNode(ControlFlowNode<TInstruction> node)
        {
            OriginalNode = node;
            Children = new TreeNodeCollection<DominatorTreeNode<TInstruction>, DominatorTreeNode<TInstruction>>(this);
        }

        /// <summary>
        /// The parent of this <see cref="DominatorTreeNode{T}"/>
        /// </summary>
        public new DominatorTreeNode<TInstruction>? Parent => (DominatorTreeNode<TInstruction>?) base.Parent;

        /// <summary>
        /// Gets the node that this tree node was derived from. 
        /// </summary>
        public ControlFlowNode<TInstruction> OriginalNode
        {
            get;
        }

        /// <summary>
        /// Gets the children of the current node.
        /// </summary>
        public IList<DominatorTreeNode<TInstruction>> Children
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
        public IEnumerable<DominatorTreeNode<TInstruction>> GetDirectChildren() =>
            Children.Where(c => c.Parent!.HasPredecessor(OriginalNode));

        /// <summary>
        /// Gets a collection of children representing all nodes that were dominated by the original node, but were not
        /// an immediate successor of the original node.
        /// </summary>
        /// <returns>The children, represented by the dominator tree nodes.</returns>
        public IEnumerable<DominatorTreeNode<TInstruction>> GetIndirectChildren() =>
            Children.Where(c => !c.Parent!.HasPredecessor(OriginalNode));

        /// <summary>
        /// Gets all the nodes that are dominated by this control flow graph node.
        /// </summary>
        /// <returns>The nodes that are dominated by this node.</returns>
        /// <remarks>
        /// Because of the nature of dominator analysis, this also includes the current node.
        /// </remarks>
        public IEnumerable<DominatorTreeNode<TInstruction>> GetDominatedNodes()
        {
            var stack = new Stack<DominatorTreeNode<TInstruction>>();
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