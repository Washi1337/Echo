using System;
using System.Collections.ObjectModel;

namespace Echo.ControlFlow.Analysis.Domination
{
    /// <summary>
    /// Represents a collection of dominator tree node children that are dominated by a single control flow graph node.
    /// </summary>
    public class DominatorTreeNodeCollection : Collection<DominatorTreeNode>
    {
        public DominatorTreeNodeCollection(DominatorTreeNode owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        public DominatorTreeNode Owner
        {
            get;
        }
        
        private void AssertNoParent(DominatorTreeNode node)
        {
            if (node.Parent != null)
                throw new ArgumentException("Cannot add a node that is already a child of another node.");
        }
        
        protected override void SetItem(int index, DominatorTreeNode item)
        {
            AssertNoParent(item);
            var old = Items[index];
            base.SetItem(index, item);
            old.Parent = null;
            item.Parent = Owner;
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.Parent = null;
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            Items[index].Parent = null;
            base.RemoveItem(index);
        }

        protected override void InsertItem(int index, DominatorTreeNode item)
        {
            AssertNoParent(item);
            item.Parent = Owner;
            base.InsertItem(index, item);
        }
    }
}