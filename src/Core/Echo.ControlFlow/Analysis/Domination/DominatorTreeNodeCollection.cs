using System;
using System.Collections.ObjectModel;
using Echo.Core.Graphing;

namespace Echo.ControlFlow.Analysis.Domination
{
    /// <summary>
    /// Represents a collection of dominator tree node children that are dominated by a single graph node.
    /// </summary>
    public class DominatorTreeNodeCollection<TNode> : Collection<DominatorTreeNode>
        where TNode : INode
    {
        private readonly DominatorTreeNode _owner;

        internal DominatorTreeNodeCollection(DominatorTreeNode owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Asserts that the provided node is not already added to another tree node.
        /// </summary>
        /// <param name="node">The node to verify.</param>
        /// <exception cref="ArgumentException">Occurs if the node is already added to another node.</exception>
        protected static void AssertNoParent(DominatorTreeNode node)
        {
            if (node.Parent != null)
                throw new ArgumentException("Cannot add a node that is already a child of another node.");
        }

        /// <inheritdoc />
        protected override void SetItem(int index, DominatorTreeNode item)
        {
            AssertNoParent(item);
            var old = Items[index];
            base.SetItem(index, item);
            old.Parent = null;
            item.Parent = _owner;
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.Parent = null;
            base.ClearItems();
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            Items[index].Parent = null;
            base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, DominatorTreeNode item)
        {
            AssertNoParent(item);
            item.Parent = _owner;
            base.InsertItem(index, item);
        }
    }
}