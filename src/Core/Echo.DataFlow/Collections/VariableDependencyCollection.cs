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
    public class VariableDependencyCollection<TContents> : IDictionary<IVariable, VariableDependency<TContents>>
    {
        private readonly Dictionary<IVariable, VariableDependency<TContents>> _entries = new();
        private readonly DataFlowNode<TContents> _owner;

        internal VariableDependencyCollection(DataFlowNode<TContents> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        /// <inheritdoc />
        public VariableDependency<TContents> this[IVariable key]
        {
            get => _entries[key];
            set
            {
                AssertDependencyValidity(value);
                Remove(key);
                Add(value);
            }
        }

        /// <inheritdoc />
        public ICollection<IVariable> Keys => _entries.Keys;

        /// <inheritdoc />
        public ICollection<VariableDependency<TContents>> Values => _entries.Values;

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <summary>
        /// Gets the total number of edges that are stored in this dependency collection.
        /// </summary>
        public int EdgeCount => _entries.Values.Sum(d => d.Count);

        /// <inheritdoc />
        public bool IsReadOnly => false;

        private void AssertDependencyValidity(VariableDependency<TContents> item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            
            if (item.Dependent is not null)
                throw new ArgumentException("Variable dependency was already added to another node.");
            
            if (item.Any(n => n.Node.ParentGraph != _owner.ParentGraph))
                throw new ArgumentException("Dependency contains data sources from another graph.");
        }
        
        /// <inheritdoc />
        public bool TryGetValue(IVariable key, out VariableDependency<TContents> value) => 
            _entries.TryGetValue(key, out value);

        /// <inheritdoc />
        void ICollection<KeyValuePair<IVariable, VariableDependency<TContents>>>.Add(
            KeyValuePair<IVariable, VariableDependency<TContents>> item)
        {
            if (item.Key != item.Value.Variable)
                throw new ArgumentException("Key value does not match the variable specified in the dependency.");
            Add(item.Value);
        }

        /// <inheritdoc />
        void IDictionary<IVariable, VariableDependency<TContents>>.Add(IVariable key, VariableDependency<TContents> value)
        {
            if (key != value.Variable)
                throw new ArgumentException("Key value does not match the variable specified in the dependency.");
            Add(value);
        }

        public void Add(VariableDependency<TContents> dependency)
        {
            AssertDependencyValidity(dependency);
            _entries.Add(dependency.Variable, dependency);
            dependency.Dependent = _owner;
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            foreach (var variable in Keys.ToArray())
                Remove(variable);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<IVariable, VariableDependency<TContents>> item) => 
            _entries.Contains(item);

        /// <inheritdoc />
        public bool ContainsKey(IVariable key) => 
            _entries.ContainsKey(key);

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<IVariable, VariableDependency<TContents>>[] array, int arrayIndex) => 
            ((ICollection)_entries).CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<IVariable, VariableDependency<TContents>> item) => 
            _entries.TryGetValue(item.Key, out var dependency) && dependency == item.Value && Remove(item.Key);

        /// <inheritdoc />
        public bool Remove(IVariable key)
        {
            if (_entries.TryGetValue(key, out var dependency))
            {
                dependency.Dependent = null;
                _entries.Remove(key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Obtains an enumerator that enumerates all recorded variable dependencies in this collection. 
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new(this);

        IEnumerator<KeyValuePair<IVariable, VariableDependency<TContents>>>
            IEnumerable<KeyValuePair<IVariable, VariableDependency<TContents>>>.GetEnumerator()
        {
            return _entries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable) _entries).GetEnumerator();

        /// <summary>
        /// Represents an enumerator that enumerates all entries in a variable dependencies collection.
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<IVariable, VariableDependency<TContents>>>
        {
            private Dictionary<IVariable, VariableDependency<TContents>>.Enumerator _enumerator;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> class.
            /// </summary>
            /// <param name="collection">The collection to enumerate.</param>
            public Enumerator(VariableDependencyCollection<TContents> collection)
            {
                _enumerator = collection._entries.GetEnumerator();
            }

            /// <inheritdoc />
            public KeyValuePair<IVariable, VariableDependency<TContents>> Current => _enumerator.Current;

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