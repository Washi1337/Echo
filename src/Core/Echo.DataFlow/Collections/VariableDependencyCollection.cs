using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Echo.Code;

namespace Echo.DataFlow.Collections
{
    /// <summary>
    /// Represents a collection of variables and their symbolic values that a node in a data flow graph depends on.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction to put in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class VariableDependencyCollection<TInstruction> : ICollection<VariableDependency<TInstruction>>
        where TInstruction : notnull
    {
        private readonly Dictionary<IVariable, VariableDependency<TInstruction>> _entries = new();
        private readonly DataFlowNode<TInstruction> _owner;

        internal VariableDependencyCollection(DataFlowNode<TInstruction> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }
        
        /// <summary>
        /// Gets or sets the variable dependency assigned to the variable.
        /// </summary>
        /// <param name="variable">The variable</param>
        public VariableDependency<TInstruction> this[IVariable variable]
        {
            get => _entries[variable];
            set
            {
                AssertDependencyValidity(value);
                Remove(variable);
                Add(value);
            }
        }

        /// <inheritdoc />
        public bool Remove(VariableDependency<TInstruction> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count => _entries.Count;

        /// <summary>
        /// Gets the total number of edges that are stored in this dependency collection.
        /// </summary>
        public int EdgeCount => _entries.Values.Sum(d => d.Count);

        /// <inheritdoc />
        public bool IsReadOnly => false;

        private void AssertDependencyValidity(VariableDependency<TInstruction> item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            
            if (item.Dependent is not null)
                throw new ArgumentException("Variable dependency was already added to another node.");
            
            if (item.Any(n => n.Node.ParentGraph != _owner.ParentGraph))
                throw new ArgumentException("Dependency contains data sources from another graph.");
        }
        
        /// <summary>
        /// Attempts to get the dependency assigned to the provided variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="dependency">When this function returns <c>true</c>, contains the dependency.</param>
        /// <returns><c>true</c> if the variable was registered as a dependency, <c>false</c> otherwise.</returns>
        public bool TryGetDependency(IVariable variable, out VariableDependency<TInstruction> dependency) => 
            _entries.TryGetValue(variable, out dependency);

        /// <summary>
        /// Adds a variable dependency to the node.
        /// </summary>
        /// <param name="dependency">The dependency to add.</param>
        public void Add(VariableDependency<TInstruction> dependency)
        {
            AssertDependencyValidity(dependency);
            _entries.Add(dependency.Variable, dependency);
            dependency.Dependent = _owner;
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            foreach (var variable in _entries.Keys.ToArray())
                Remove(variable);
        }

        /// <inheritdoc />
        public bool Contains(VariableDependency<TInstruction> item)
        {
            return item is not null
                   && _entries.TryGetValue(item.Variable, out var dependency) 
                   && dependency == item;
        }

        /// <summary>
        /// Determines whether the provided variable is registered as a dependency.
        /// </summary>
        /// <param name="variable">The dependency.</param>
        /// <returns></returns>
        public bool ContainsVariable(IVariable variable) => 
            _entries.ContainsKey(variable);

        /// <inheritdoc />
        public void CopyTo(VariableDependency<TInstruction>[] array, int arrayIndex) => 
            _entries.Values.CopyTo(array, arrayIndex);

        /// <summary>
        /// Unregisters a variable as a dependency. 
        /// </summary>
        /// <param name="variable">The variable to unregister.</param>
        /// <returns><c>true</c> if the variable was registered before and is now unregistered, <c>false</c> otherwise.</returns>
        public bool Remove(IVariable variable)
        {
            if (_entries.TryGetValue(variable, out var dependency))
            {
                dependency.Dependent = null;
                _entries.Remove(variable);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Obtains a collection of variables that are registered in the dependency. 
        /// </summary>
        /// <returns>The variables.</returns>
        public IEnumerable<IVariable> GetRegisteredVariables() => _entries.Keys;

        /// <summary>
        /// Obtains an enumerator that enumerates all recorded variable dependencies in this collection. 
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new(this);

        IEnumerator<VariableDependency<TInstruction>> IEnumerable<VariableDependency<TInstruction>>.GetEnumerator() => 
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable) _entries).GetEnumerator();

        /// <summary>
        /// Represents an enumerator that enumerates all entries in a variable dependencies collection.
        /// </summary>
        public struct Enumerator : IEnumerator<VariableDependency<TInstruction>>
        {
            private Dictionary<IVariable, VariableDependency<TInstruction>>.Enumerator _enumerator;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> class.
            /// </summary>
            /// <param name="collection">The collection to enumerate.</param>
            public Enumerator(VariableDependencyCollection<TInstruction> collection)
            {
                _enumerator = collection._entries.GetEnumerator();
            }

            /// <inheritdoc />
            public VariableDependency<TInstruction> Current => _enumerator.Current.Value;

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