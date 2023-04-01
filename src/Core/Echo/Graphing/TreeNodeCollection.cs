using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Echo.Graphing
{
    /// <summary>
    /// Represents a collection of tree node children
    /// </summary>
    /// <typeparam name="TParent">The type of the parent</typeparam>
    /// <typeparam name="TChild">The node to create a collection of</typeparam>
    public class TreeNodeCollection<TParent, TChild> : Collection<TChild>
        where TParent : TreeNodeBase
        where TChild : TreeNodeBase
    {
        private readonly TParent _owner;

        /// <summary>
        /// Creates a new tree node collection with the specified <paramref name="owner"/>
        /// </summary>
        /// <param name="owner">The owner whose children this collection represents</param>
        public TreeNodeCollection(TParent owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Asserts that the provided node is not already added to another tree node.
        /// </summary>
        /// <param name="node">The node to verify.</param>
        /// <exception cref="ArgumentException">Occurs if the node is already added to another node.</exception>
        protected static void AssertNoParent(TChild node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));
            if (node.Parent != null)
                throw new ArgumentException("Cannot add a node that is already a child of another node.");
        }

        /// <inheritdoc />
        protected override void SetItem(int index, TChild item)
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
        protected override void InsertItem(int index, TChild item)
        {
            AssertNoParent(item);
            item.Parent = _owner;
            base.InsertItem(index, item);
        }
    }
}