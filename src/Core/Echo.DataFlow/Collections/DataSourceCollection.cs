using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Collections
{
    /// <summary>
    /// Represents a set of data sources for a symbolic value.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to store in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class DataSourceCollection<TContents> : ICollection<DataFlowNode<TContents>>
    {
        private readonly HashSet<DataFlowNode<TContents>> _items;

        /// <summary>
        /// Creates a new empty data source collection.
        /// </summary>
        /// <param name="owner">The symbolic value that owns this data source collection.</param>
        internal DataSourceCollection(SymbolicValue<TContents> owner, DataFlowEdgeType type)
            : this(owner, Enumerable.Empty<DataFlowNode<TContents>>())
        {
            Type = type;
        }

        /// <summary>
        /// Creates a new data source collection and adds the provided elements to the list. 
        /// </summary>
        /// <param name="owner">The symbolic value that owns this data source collection.</param>
        /// <param name="items">The items to add to the collection.</param>
        public DataSourceCollection(SymbolicValue<TContents> owner, IEnumerable<DataFlowNode<TContents>> items)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _items = new HashSet<DataFlowNode<TContents>>(items);
        }

        /// <summary>
        /// Gets the owner of the collection.
        /// </summary>
        public SymbolicValue<TContents> Owner
        {
            get;
        }

        public DataFlowEdgeType Type
        {
            get;
        }

        /// <inheritdoc />
        public int Count => _items.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds a data source to the collection.
        /// </summary>
        /// <param name="item">The source to add.</param>
        /// <returns><c>true</c> if the data source was added successfully, <c>false</c> if the data source was already present in the collection.</returns>
        public bool Add(DataFlowNode<TContents> item)
        {
            if (item == null)
                throw new ArgumentNullException();
            if (Owner.Dependant != null && item.ParentGraph != Owner.Dependant.ParentGraph)
                throw new ArgumentException("Data source is not added to the same graph.");

            if (_items.Add(item))
            {
                item.Dependants.Add(Owner.Dependant);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        void ICollection<DataFlowNode<TContents>>.Add(DataFlowNode<TContents> item) => Add(item);

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var item in this.ToArray())
                Remove(item);
        }

        /// <inheritdoc />
        public bool Contains(DataFlowNode<TContents> item) => _items.Contains(item);

        /// <inheritdoc />
        public void CopyTo(DataFlowNode<TContents>[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(DataFlowNode<TContents> item)
        {
            if (_items.Remove(item))
            {
                item.Dependants.Remove(Owner.Dependant);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public IEnumerator<DataFlowNode<TContents>> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}