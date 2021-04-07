using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Echo.Core.Code;

namespace Echo.DataFlow.Collections
{
    /// <summary>
    /// Represents a collection of variables and their symbolic values that a node in a data flow graph depends on.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class VariableDependencyCollection<TContents> : IDictionary<IVariable, DataDependency<TContents>>
    {
        private readonly Dictionary<IVariable, DataDependency<TContents>> _entries =
            new Dictionary<IVariable, DataDependency<TContents>>();
        private readonly DataFlowNode<TContents> _owner;

        internal VariableDependencyCollection(DataFlowNode<TContents> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        /// <inheritdoc />
        public DataDependency<TContents> this[IVariable key]
        {
            get => _entries[key];
            set
            {
                Remove(key);
                Add(key, value);
            }
        }

        /// <inheritdoc />
        public ICollection<IVariable> Keys => _entries.Keys;

        /// <inheritdoc />
        public ICollection<DataDependency<TContents>> Values => _entries.Values;

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <summary>
        /// Gets the total number of edges that are stored in this dependency collection.
        /// </summary>
        public int EdgeCount => throw new NotImplementedException();

        /// <inheritdoc />
        public bool IsReadOnly => false;

        private void AssertDependencyValidity(DataDependency<TContents> item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            
            if (item.Dependant != null)
                throw new ArgumentException("Variable dependency was already added to another node.");
            
            if (item.Any(n => n.Node.ParentGraph != _owner.ParentGraph))
                throw new ArgumentException("Dependency contains data sources from another graph.");
        }
        
        /// <inheritdoc />
        public bool TryGetValue(IVariable key, out DataDependency<TContents> value) => 
            _entries.TryGetValue(key, out value);

        /// <inheritdoc />
        public void Add(KeyValuePair<IVariable, DataDependency<TContents>> item) => 
            Add(item.Key, item.Value);

        /// <inheritdoc />
        public void Add(IVariable key, DataDependency<TContents> value)
        {
            AssertDependencyValidity(value);
            _entries.Add(key, value);
            value.Dependant = _owner;
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            foreach (var variable in Keys.ToArray())
                Remove(variable);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<IVariable, DataDependency<TContents>> item) => 
            _entries.Contains(item);

        /// <inheritdoc />
        public bool ContainsKey(IVariable key) => 
            _entries.ContainsKey(key);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<IVariable, DataDependency<TContents>>[] array, int arrayIndex) => 
            ((ICollection)_entries).CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<IVariable, DataDependency<TContents>> item) => 
            _entries.TryGetValue(item.Key, out var dependency) && dependency == item.Value && Remove(item.Key);

        /// <inheritdoc />
        public bool Remove(IVariable key)
        {
            if (_entries.TryGetValue(key, out var dependency))
            {
                dependency.Dependant = null;
                _entries.Remove(key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Obtains an enumerator that enumerates all recorded variable dependencies in this collection. 
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<KeyValuePair<IVariable, DataDependency<TContents>>>
            IEnumerable<KeyValuePair<IVariable, DataDependency<TContents>>>.GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable) _entries).GetEnumerator();

        /// <summary>
        /// Represents an enumerator that enumerates all entries in a variable dependencies collection.
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<IVariable, DataDependency<TContents>>>
        {
            private Dictionary<IVariable,DataDependency<TContents>>.Enumerator _enumerator;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> class.
            /// </summary>
            /// <param name="collection">The collection to enumerate.</param>
            public Enumerator(VariableDependencyCollection<TContents> collection)
            {
                _enumerator = collection._entries.GetEnumerator();
            }

            /// <inheritdoc />
            public KeyValuePair<IVariable, DataDependency<TContents>> Current => _enumerator.Current;

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext() => _enumerator.MoveNext();

            /// <inheritdoc />
            public void Reset() => ((IEnumerator) _enumerator).Reset();

            /// <inheritdoc />
            public void Dispose() => _enumerator.Dispose();
        }
    }
}