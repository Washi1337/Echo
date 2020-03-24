using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Collections
{
    /// <summary>
    /// Represents a collection of dependencies allocated on a stack for a node in a data flow graph.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class StackDependencyCollection<TContents> : Collection<DataDependency<TContents>>
    {
        private readonly DataFlowNode<TContents> _owner;

        /// <summary>
        /// Creates a new dependency collection for a node.
        /// </summary>
        /// <param name="owner">The owner node.</param>
        internal StackDependencyCollection(DataFlowNode<TContents> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <summary>
        /// Gets the total number of edges that are stored in this dependency collection.
        /// </summary>
        public int EdgeCount => Items.Sum(i => i.DataSources.Count);

        private void AssertDependencyValidity(DataDependency<TContents> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            
            if (item.Dependant != null)
                throw new ArgumentException("Stack dependency was already added to another node.");
            
            if (item.DataSources.Any(n => n.ParentGraph != _owner.ParentGraph))
                throw new ArgumentException("Dependency contains data sources from another graph.");
        }

        /// <inheritdoc />
        protected override void InsertItem(int index, DataDependency<TContents> item)
        {
            AssertDependencyValidity(item);
            base.InsertItem(index, item);
            item.Dependant = _owner;
        }

        /// <inheritdoc />
        protected override void SetItem(int index, DataDependency<TContents> item)
        {
            AssertDependencyValidity(item);
            
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

        /// <summary>
        /// Gets the enumerator for this stack dependency collection.
        /// </summary>
        /// <returns></returns>
        public new Enumerator GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// Represents an enumerator for a stack dependency collection.
        /// </summary>
        public struct Enumerator : IEnumerator<DataDependency<TContents>>
        {
            private readonly StackDependencyCollection<TContents> _collection;
            private DataDependency<TContents> _current;
            private int _index;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> structure.
            /// </summary>
            /// <param name="collection">The collection to enumerate.</param>
            public Enumerator(StackDependencyCollection<TContents> collection)
                : this()
            {
                _collection = collection;
                _index = -1;
                _current = null;
            }

            /// <inheritdoc />
            public DataDependency<TContents> Current => _current;

            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                _index++;
                if (_index < _collection.Count)
                {
                    _current = _collection[_index];
                    return true;
                }

                _current = null;
                return false;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _index = -1;
                _current = null;
            }

            /// <inheritdoc />
            public void Dispose()
            {
            }
        }
    }
}