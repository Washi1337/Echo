using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Echo.ControlFlow.Regions;

namespace Echo.ControlFlow.Collections
{
    /// <summary>
    /// Represents a collection of nodes that are put into a control flow region.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class RegionNodeCollection<TInstruction> : Collection<ControlFlowNode<TInstruction>>
        where TInstruction : notnull
    {
        private readonly IScopeControlFlowRegion<TInstruction> _owner;

        /// <summary>
        /// Creates a new instance of the <see cref="RegionNodeCollection{TInstruction}"/> class.
        /// </summary>
        /// <param name="owner">The region owning the collection of nodes.</param>
        public RegionNodeCollection(IScopeControlFlowRegion<TInstruction> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        private void AssertNodeValidity(ControlFlowNode<TInstruction> node)
        {
            if (_owner.ParentGraph is null)
                throw new InvalidOperationException("Region is not part of a graph.");
            if (node.ParentGraph is null)
                throw new ArgumentException("Node is not added to the same graph");
            if (node.ParentRegion != _owner.ParentGraph)
                throw new ArgumentException("Node is already added to another region.");
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, ControlFlowNode<TInstruction> item)
        {
            AssertNodeValidity(item);
            base.InsertItem(index, item);
            item.ParentRegion = _owner;
        }

        /// <inheritdoc />
        protected override void SetItem(int index, ControlFlowNode<TInstruction> item)
        {
            AssertNodeValidity(item);
            item.ParentRegion = _owner.ParentGraph;
            base.SetItem(index, item);
            item.ParentRegion = _owner;
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            Items[index].ParentRegion = _owner.ParentGraph;
            base.RemoveItem(index);
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.ParentRegion = _owner.ParentGraph;
            base.ClearItems();
        }

        /// <summary>
        /// Adds a collection of nodes to the node collection.
        /// </summary>
        /// <param name="items">The nodes to add.</param>
        public void AddRange(IEnumerable<ControlFlowNode<TInstruction>> items)
        {
            var nodes = items as ControlFlowNode<TInstruction>[] ?? items.ToArray();
            foreach (var item in nodes)
                AssertNodeValidity(item);
            foreach (var item in nodes)
                Add(item);
        }
    }
}