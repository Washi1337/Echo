using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Echo.Code;

namespace Echo.DataFlow.Emulation
{
    /// <summary>
    /// Represents a symbolic value that resides in memory. 
    /// </summary>
    public sealed class SymbolicValue<T> : ISet<DataSource<T>>
    {
        // -------------------------
        // Implementation rationale:
        // -------------------------
        //
        // To prevent allocations of "big" empty set objects, we delay initialization of it until we actually need
        // to store more than one data source. This is worth the extra steps in pattern matching, since there is
        // going to be a lot of instances of this type. For most architectures and functions written in these
        // languages, instructions only have zero or one data source per data dependency, and would therefore not
        // need special complex heap allocated objects to store zero or just one data source.
        //
        // For this reason, the following "list object" field can have three possible values:
        //    - null:                     The dependency has no known data sources.
        //    - DataSource<T>:            The dependency has a single data source.
        //    - HashSet<DataSource<T>>:   The dependency has multiple data sources.
        
        private object _listObject;
        
        /// <summary>
        /// Creates a new symbolic value with no data sources.
        /// </summary>
        public SymbolicValue()
        {
            _listObject = null;
        }

        /// <summary>
        /// Creates a new symbolic value with a single data source.
        /// </summary>
        /// <param name="dataSource">The data source of the symbolic value.</param>
        public SymbolicValue(DataSource<T> dataSource)
        {
            _listObject = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        }
        
        /// <summary>
        /// Creates a new symbolic value with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources of the symbolic value.</param>
        public SymbolicValue(IEnumerable<DataSource<T>> dataSources)
        {
            _listObject = new HashSet<DataSource<T>>(dataSources);
        }

        /// <summary>
        /// Merges two data dependencies into one symbolic value.
        /// </summary>
        public SymbolicValue(SymbolicValue<T> left, SymbolicValue<T> right)
        {
            int totalCount = left.Count + right.Count;
            switch (totalCount)
            {
                case 0:
                    _listObject = null;
                    break;

                case 1:
                    _listObject = left._listObject ?? right._listObject;
                    break;

                default:
                    var set = new HashSet<DataSource<T>>(left);
                    set.UnionWith(right);
                    _listObject = set;
                    break;
            }
        }
        
        /// <inheritdoc />
        public int Count => _listObject switch
        {
            null => 0,
            DataSource<T> _ => 1,
            ICollection<DataSource<T>> collection => collection.Count,
            _ => throw new InvalidOperationException("Data dependency is in an invalid state.")
        };

        /// <inheritdoc />
        public bool IsReadOnly => true;
        
        /// <summary>
        /// Gets a value indicating whether the data dependency has any known data sources. 
        /// </summary>
        public bool HasKnownDataSources => Count > 0;
        
        /// <summary>
        /// Creates a new symbolic value referencing the first stack value produced by the provided node. 
        /// </summary>
        /// <param name="node">The node producing the value.</param>
        /// <returns>The symbolic value.</returns>
        public static SymbolicValue<T> CreateStackValue(DataFlowNode<T> node) => 
            new(new StackDataSource<T>(node, 0));

        /// <summary>
        /// Creates a new symbolic value referencing a stack value produced by the provided node. 
        /// </summary>
        /// <param name="node">The node producing the value.</param>
        /// <param name="slotIndex">The index of the stack value that was produced by the node.</param>
        /// <returns>The symbolic value.</returns>
        public static SymbolicValue<T> CreateStackValue(DataFlowNode<T> node, int slotIndex) => 
            new(new StackDataSource<T>(node, slotIndex));

        /// <summary>
        /// Creates a new symbolic value referencing a variable value assigned by the provided node. 
        /// </summary>
        /// <param name="node">The node assigning the value.</param>
        /// <param name="variable">The variable that was assigned a value.</param>
        /// <returns>The symbolic value.</returns>
        public static SymbolicValue<T> CreateVariableValue(DataFlowNode<T> node, IVariable variable) => 
            new(new VariableDataSource<T>(node, variable));

        /// <summary>
        /// Interprets the symbolic value as a collection of stack data sources.
        /// </summary>
        /// <returns>The stack data sources.</returns>
        public IEnumerable<StackDataSource<T>> AsStackValue() => this.Cast<StackDataSource<T>>();
        
        /// <summary>
        /// Interprets the symbolic value as a collection of variable data sources.
        /// </summary>
        /// <returns>The variable data sources.</returns>
        public IEnumerable<VariableDataSource<T>> AsVariableValue() => this.Cast<VariableDataSource<T>>();

        private static bool ThrowInvalidStateException() => 
            throw new InvalidOperationException("Data dependency is in an invalid state.");

        private void AssertIsWritable()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Data dependency is in a read-only state.");
        }

        /// <inheritdoc />
        public bool Add(DataSource<T> item)
        {
            AssertIsWritable();

            switch (_listObject)
            {
                case null:
                    _listObject = item;
                    return true;

                case DataSource<T> node:
                    if (node == item)
                        return false;
                    
                    _listObject = new HashSet<DataSource<T>>
                    {
                        node,
                        item
                    };
                    return true;

                case ISet<DataSource<T>> nodes:
                    return nodes.Add(item);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<DataSource<T>> other)
        {
            AssertIsWritable();
            
            foreach (var node in other)
                Remove(node);
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<DataSource<T>> other)
        {
            AssertIsWritable();
            
            var set = new HashSet<DataSource<T>>(other);
            foreach (var item in this)
            {
                if (!set.Contains(item))
                    Remove(item);
            }
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<DataSource<T>> other)
        {
            switch (_listObject)
            {
                case null:
                    return other.Any();
                
                case DataSource<T> node:
                    bool containsElement = false;
                    foreach (var item in other)
                    {
                        if (node == item)
                            containsElement = true;
                        else if (containsElement)
                            return true;
                    }

                    return false;
                    
                case ISet<DataSource<T>> nodes:
                    return nodes.IsProperSubsetOf(other);
                
                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<DataSource<T>> other) => _listObject switch
        {
            null => false,
            DataSource<T> _ => !other.Any(),
            ISet<DataSource<T>> nodes => nodes.IsProperSupersetOf(other),
            _ => ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<DataSource<T>> other) => _listObject switch
        {
            null => true,
            DataSource<T> node => other.Contains(node),
            ISet<DataSource<T>> nodes => nodes.IsSubsetOf(other),
            _ =>  ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<DataSource<T>> other)
        {
            switch (_listObject)
            {
                case null:
                    return !other.Any();

                case DataSource<T> node:
                {
                    using var enumerator = other.GetEnumerator();
                    return !enumerator.MoveNext() || enumerator.Current == node && !enumerator.MoveNext();
                }

                case ISet<DataSource<T>> nodes:
                    return nodes.IsSupersetOf(other);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<DataSource<T>> other) => _listObject switch
        {
            null => false,
            DataSource<T> node => other.Contains(node),
            ISet<DataSource<T>> nodes => nodes.Overlaps(other),
            _ => ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<DataSource<T>> other)
        {
            if (other is SymbolicValue<T> otherSymbolicValue)
                return SetEqualsFast(otherSymbolicValue);
            
            switch (_listObject)
            {
                case null:
                    return !other.Any();
                
                case DataSource<T> node:
                {
                    using var enumerator = other.GetEnumerator();
                    return enumerator.MoveNext() && node == enumerator.Current && !enumerator.MoveNext();
                }

                case ISet<DataSource<T>> nodes:
                    return nodes.SetEquals(other);

                default:
                    return ThrowInvalidStateException();
            }
        }
        
        private bool SetEqualsFast(SymbolicValue<T> other)
        {
            switch (_listObject)
            {
                case null:
                    return other._listObject is null;
                
                case DataSource<T> node:
                    return other._listObject is DataSource<T> otherSource && node == otherSource;

                case ISet<DataSource<T>> nodes:
                    return other._listObject is ISet<DataSource<T>> otherSet && nodes.SetEquals(otherSet);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<DataSource<T>> other)
        {
            foreach (var item in other)
            {
                if (Contains(item))
                    Remove(item);
                else
                    Add(item);
            }
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<DataSource<T>> other)
        {
            AssertIsWritable();
            foreach (var node in other)
                Add(node);
        }

        void ICollection<DataSource<T>>.Add(DataSource<T> item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            AssertIsWritable();
            switch (_listObject)
            {
                case null:
                    break;

                case DataSource<T> node:
                    Remove(node);
                    break;

                case ICollection<DataSource<T>> nodes:
                    foreach (var node in nodes.ToArray())
                        Remove(node);
                    break;

                default:
                    ThrowInvalidStateException();
                    break;
            }

            _listObject = null;
        }

        /// <inheritdoc />
        public bool Contains(DataSource<T> item) => _listObject switch
        {
            null => false,
            DataSource<T> node => item == node,
            ICollection<DataSource<T>> nodes => nodes.Contains(item),
            _ => ThrowInvalidStateException()
        };

        /// <inheritdoc />
        public void CopyTo(DataSource<T>[] array, int arrayIndex)
        {
            switch (_listObject)
            {
                case null:
                    break;
                
                case DataSource<T> node:
                    array[arrayIndex] = node;
                    break;
                
                case ICollection<DataSource<T>> nodes:
                    nodes.CopyTo(array, arrayIndex);
                    break;
                
                default:
                    ThrowInvalidStateException();
                    break;
            }
        }

        /// <summary>
        /// Removes all data sources that are related to the specified node.
        /// </summary>
        /// <param name="node">The node to remove all data sources from.</param>
        /// <returns><c>true</c> if any data source was removed, <c>false</c> otherwise.</returns>
        public bool Remove(DataFlowNode<T> node)
        {
            AssertIsWritable();
            
            var sourcesToRemove = new List<DataSource<T>>();

            foreach (var source in this)
            {
                if (source.Node == node)
                    sourcesToRemove.Add(source);
            }

            foreach (var source in sourcesToRemove)
                Remove(source);

            return sourcesToRemove.Count > 0;
        }
        
        /// <inheritdoc />
        public bool Remove(DataSource<T> item) 
        {
            AssertIsWritable();
            
            switch (_listObject)
            {
                case null:
                    return false;

                case DataSource<T> node:
                    if (node == item)
                    {
                        _listObject = null;
                        return true;
                    }

                    return false;

                case ICollection<DataSource<T>> nodes:
                    return nodes.Remove(item);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <summary>
        /// Gets a collection of nodes that were referenced by all data sources in this data dependency.
        /// </summary>
        public IEnumerable<DataFlowNode<T>> GetNodes() => this
            .Select(source => source.Node)
            .Distinct();
        
        /// <inheritdoc />
        public override string ToString() => _listObject switch
        {
            null => "?",
            DataSource<T> node => node.ToString(),
            IEnumerable<DataSource<T>> collection => $"({string.Join(" | ", collection)})",
            _ => ThrowInvalidStateException().ToString()
        };

        /// <summary>
        /// Returns an enumerator that iterates over all data sources the data dependency defines.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new (this);

        IEnumerator<DataSource<T>> IEnumerable<DataSource<T>>.GetEnumerator() => GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Provides a mechanism for enumerating all data sources within a single symbolic value. 
        /// </summary>
        public struct Enumerator : IEnumerator<DataSource<T>>
        {
            private readonly SymbolicValue<T> _collection;
            private HashSet<DataSource<T>>.Enumerator _setEnumerator;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> structure.
            /// </summary>
            /// <param name="collection">The data dependency to enumerate the data sources for.</param>
            public Enumerator(SymbolicValue<T> collection)
            {
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                if (collection._listObject is HashSet<DataSource<T>> nodes)
                    _setEnumerator = nodes.GetEnumerator();
                Current = null;
            }

            /// <inheritdoc />
            public DataSource<T> Current
            {
                get;
                private set;
            }

            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_collection is null)
                    return false;
                
                switch (_collection._listObject)
                {
                    case null:
                        return false;
                    
                    case DataSource<T> source:
                        if (Current is null)
                        {
                            Current = source;
                            return true;
                        }

                        break;
                    
                    default:
                        if (_setEnumerator.MoveNext())
                        {
                            Current = _setEnumerator.Current;
                            return true;
                        }
                        break;
                }
                
                Current = null;
                return false;
            }

            /// <inheritdoc />
            public void Reset() => throw new NotSupportedException();

            /// <inheritdoc />
            public void Dispose() => _setEnumerator.Dispose();
        }
    }
}