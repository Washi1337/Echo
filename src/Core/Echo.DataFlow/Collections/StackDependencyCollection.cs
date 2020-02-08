using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Collections
{
    /// <summary>
    /// Represents a collection of dependencies allocated on a stack for a node in a data flow graph.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class StackDependencyCollection<TContents> : Collection<SymbolicValue<TContents>>
    {
        /// <summary>
        /// Creates a new dependency collection for a node.
        /// </summary>
        /// <param name="owner">The owner node.</param>
        internal StackDependencyCollection(DataFlowNode<TContents> owner)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Gets the owner node of this dependency collection.
        /// </summary>
        public DataFlowNode<TContents> Owner
        {
            get;
        }

        private static void AssertHasNoOwner(SymbolicValue<TContents> item)
        {
            if (item.Dependant != null)
                throw new InvalidOperationException("Dependency was already added to another node.");
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, SymbolicValue<TContents> item)
        {
            AssertHasNoOwner(item);
            base.InsertItem(index, item);
            item.Dependant = Owner;
        }

        /// <inheritdoc />
        protected override void SetItem(int index, SymbolicValue<TContents> item)
        {
            AssertHasNoOwner(item);
            RemoveAt(index);
            Insert(index, item);
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            var item = Items[index];
            base.RemoveItem(index);
            item.Dependant = null;
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            while (Items.Count > 0)
                RemoveAt(0);
        }
    }
}