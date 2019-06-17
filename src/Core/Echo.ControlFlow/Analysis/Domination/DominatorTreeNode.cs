using System;
using System.Collections.Generic;

namespace Echo.ControlFlow.Analysis.Domination
{
    /// <summary>
    /// Represents a single node in a dominator tree of a control flow graph. 
    /// </summary>
    /// <remarks>
    /// It stores the original control flow graph node from which this tree node was inferred, as well a reference to its
    /// immediate dominator, and the node it immediate dominates.
    /// </remarks>
    public class DominatorTreeNode
    {
        public DominatorTreeNode(INode node)
        {
            OriginalNode = node ?? throw new ArgumentNullException(nameof(node));
            Children  = new DominatorTreeNodeCollection(this);
        }

        /// <summary>
        /// Gets the node that this tree node was derived from. 
        /// </summary>
        public INode OriginalNode
        {
            get;
        }

        /// <summary>
        /// Gets the parent of the tree node. That is, the immediate dominator of the original control flow graph node.
        /// </summary>
        public DominatorTreeNode Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a collection of child nodes that are immediately dominated by the original control flow graph node.
        /// </summary>
        public IList<DominatorTreeNode> Children
        {
            get;
        }
    }
}