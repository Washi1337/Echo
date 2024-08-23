using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Echo.ControlFlow.Regions;

namespace Echo.ControlFlow.Collections
{
    /// <summary>
    /// Represents a collection of regions found in a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    /// <typeparam name="TRegion">The type of the region to store.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class RegionCollection<TInstruction, TRegion> : Collection<TRegion>
        where TInstruction : notnull
        where TRegion : ControlFlowRegion<TInstruction>
    {
        private readonly IControlFlowRegion<TInstruction> _owner;

        /// <summary>
        /// Creates a new instance of the <see cref="RegionCollection{TInstruction, TRegion}"/> class.
        /// </summary>
        /// <param name="owner">The owner of the sub regions.</param>
        public RegionCollection(IControlFlowRegion<TInstruction> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        private void AssertRegionValidity(TRegion item)
        {
            if (item.ParentRegion is {})
                throw new ArgumentException("Region is already part of another graph.");
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, TRegion item)
        {
            AssertRegionValidity(item);
            base.InsertItem(index, item);
            item.ParentRegion = _owner;
        }

        /// <inheritdoc />
        protected override void SetItem(int index, TRegion item)
        {
            AssertRegionValidity(item);
            Items[index].ParentRegion = null;
            base.SetItem(index, item);
            item.ParentRegion = _owner;
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.ParentRegion = null;
            base.ClearItems();
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            Items[index].ParentRegion = null;
            base.RemoveItem(index);
        }
    }
}