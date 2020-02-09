using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Echo.DataFlow.Collections
{
    /// <summary>
    /// Represents a set of data sources for a symbolic value.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to store in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class DataSourceCollection<TContents> : ISet<DataFlowNode<TContents>>
    {
        private readonly HashSet<DataFlowNode<TContents>> _items;

        /// <summary>
        /// Creates a new empty data source collection.
        /// </summary>
        /// <param name="owner">The symbolic value that owns this data source collection.</param>
        internal DataSourceCollection(DataDependency<TContents> owner)
            : this(owner, Enumerable.Empty<DataFlowNode<TContents>>())
        {
        }

        /// <summary>
        /// Creates a new data source collection and adds the provided elements to the list. 
        /// </summary>
        /// <param name="owner">The symbolic value that owns this data source collection.</param>
        /// <param name="items">The items to add to the collection.</param>
        public DataSourceCollection(DataDependency<TContents> owner, IEnumerable<DataFlowNode<TContents>> items)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _items = new HashSet<DataFlowNode<TContents>>(items);
        }

        /// <summary>
        /// Gets the owner of the collection.
        /// </summary>
        public DataDependency<TContents> Owner
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
        public void ExceptWith(IEnumerable<DataFlowNode<TContents>> other)
        {
            foreach (var item in other)
                Remove(item);
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<DataFlowNode<TContents>> other)
        {
            var set = new HashSet<DataFlowNode<TContents>>(other);
            foreach (var item in this)
            {
                if (!set.Contains(item))
                    Remove(item);
            }
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<DataFlowNode<TContents>> other) => 
            _items.IsProperSubsetOf(other);

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<DataFlowNode<TContents>> other) => 
            _items.IsProperSupersetOf(other);

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<DataFlowNode<TContents>> other) => 
            _items.IsSubsetOf(other);

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<DataFlowNode<TContents>> other) => 
            _items.IsSupersetOf(other);

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<DataFlowNode<TContents>> other) => 
            _items.Overlaps(other);

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<DataFlowNode<TContents>> other) => 
            _items.SetEquals(other);

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<DataFlowNode<TContents>> other)
        {
            var items = other as DataFlowNode<TContents>[] ?? other.ToArray();
            var intersection = this.Intersect(items);
            UnionWith(items);
            ExceptWith(intersection);
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<DataFlowNode<TContents>> other)
        {
            foreach (var item in other)
                Add(item);
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