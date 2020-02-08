using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Collections
{
    /// <summary>
    /// Represents a collection of variables and their symbolic values that a node in a data flow graph depends on.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in each node.</typeparam>
    public class VariableDependencyCollection<TContents> : IDictionary<IVariable, SymbolicValue<TContents>>
    {
        private readonly IDictionary<IVariable, SymbolicValue<TContents>> _entries = new Dictionary<IVariable, SymbolicValue<TContents>>();

        /// <summary>
        /// Creates a new dependency collection for a node.
        /// </summary>
        /// <param name="owner">The owner node.</param>
        internal VariableDependencyCollection(DataFlowNode<TContents> owner)
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

        /// <inheritdoc />
        public SymbolicValue<TContents> this[IVariable key]
        {
            get => _entries[key];
            set
            {
                if (key is null || value is null)
                    throw new ArgumentNullException();
                AssertHasNoOwner(value);
                
                Remove(key);
                Add(key, value);
            }
        }

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _entries.IsReadOnly;

        /// <inheritdoc />
        public ICollection<IVariable> Keys => _entries.Keys;

        /// <inheritdoc />
        public ICollection<SymbolicValue<TContents>> Values => _entries.Values;

        /// <inheritdoc />
        public bool TryGetValue(IVariable key, out SymbolicValue<TContents> value) => 
            _entries.TryGetValue(key, out value);


        private static void AssertHasNoOwner(SymbolicValue<TContents> item)
        {
            if (item.Dependant != null)
                throw new InvalidOperationException("Dependency was already added to another node.");
        }
        
        /// <inheritdoc />
        public void Add(KeyValuePair<IVariable, SymbolicValue<TContents>> item)
        {
            if (item.Key is null || item.Value is null)
                throw new ArgumentNullException();
            AssertHasNoOwner(item.Value);
            
            _entries.Add(item);
            item.Value.Dependant = Owner;
        }

        /// <inheritdoc />
        public void Add(IVariable key, SymbolicValue<TContents> value) => 
            _entries.Add(new KeyValuePair<IVariable, SymbolicValue<TContents>>(key, value));

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var key in Keys.ToArray())
                Remove(key);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<IVariable, SymbolicValue<TContents>>[] array, int arrayIndex) =>
            _entries.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(KeyValuePair<IVariable, SymbolicValue<TContents>> item)
        {
            return _entries.TryGetValue(item.Key, out var value) && item.Value == value && Remove(item.Key);
        }

        /// <inheritdoc />
        public bool Remove(IVariable key)
        {
            if (_entries.TryGetValue(key, out var value))
            {
                Remove(key);
                value.Dependant = null;
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<IVariable, SymbolicValue<TContents>> item) => 
            _entries.Contains(item);

        /// <inheritdoc />
        public bool ContainsKey(IVariable key) =>
            _entries.ContainsKey(key);

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<IVariable, SymbolicValue<TContents>>> GetEnumerator() => 
            _entries.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _entries).GetEnumerator();
    }
}