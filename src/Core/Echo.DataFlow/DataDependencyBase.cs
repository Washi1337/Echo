using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Echo.DataFlow
{
    /// <summary>
    /// Provides a base for a data dependency,  which is a set of one or more data flow nodes where the symbolic value
    /// might pull data from.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in a data flow node.</typeparam>
    public abstract class DataDependencyBase<TContents> : IDataDependency, ISet<DataFlowNode<TContents>>
    {
        // -------------------------
        // Implementation rationale:
        // -------------------------
        //
        // To prevent allocations of big set objects, we delay initialization of it until we actually need to store
        // more than one data source. This is worth the extra steps in pattern matching, since there is going to be
        // a lot of instances of this type. For most architectures and functions written in these languages,
        // instructions only have zero or one data source per data dependency, and would therefore not need 
        // special complex heap allocated objects to store zero or just one data source.
        //
        // For this reason, the following "list object" field can have three possible values:
        //    - null:                             The dependency has no known data sources.
        //    - DataFlowNode<TContents>:          The dependency has a single data source.
        //    - HashSet<DataFlowNode<TContents>>: The dependency has multiple data sources.
        
        private object _listObject;

        /// <summary>
        /// Creates a new data dependency with no data sources.
        /// </summary>
        protected DataDependencyBase()
        {
            _listObject = null;
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        protected DataDependencyBase(DataFlowNode<TContents> dataSource)
        {
            _listObject = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources.</param>
        protected DataDependencyBase(params DataFlowNode<TContents>[] dataSources)
            : this(dataSources.AsEnumerable())
        {
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources.</param>
        protected DataDependencyBase(IEnumerable<DataFlowNode<TContents>> dataSources)
        {
            _listObject = new HashSet<DataFlowNode<TContents>>(dataSources);
        }

        /// <summary>
        /// Gets a collection of data sources this data dependency might pull data from.
        /// </summary>
        [Obsolete("This property was inlined into the "
                  + nameof(DataDependencyBase<TContents>) + " class, which now implements the "
                  + nameof(ISet<DataFlowNode<TContents>>) + " interface. Use the data dependency object "
                  + "directly to iterate over all data sources.")]
        public ISet<DataFlowNode<TContents>> DataSources => this;

        /// <inheritdoc />
        bool ICollection<DataFlowNode<TContents>>.IsReadOnly => false;

        /// <inheritdoc />
        public int Count => _listObject switch
        {
            null => 0,
            DataFlowNode<TContents> _ => 1,
            ICollection<DataFlowNode<TContents>> collection => collection.Count,
            _ => throw new InvalidOperationException("Data dependency is in an invalid state.")
        };

        /// <summary>
        /// Gets a value indicating whether the data dependency has any known data sources. 
        /// </summary>
        public bool HasKnownDataSources => Count > 0;

        IEnumerable<IDataFlowNode> IDataDependency.GetDataSources() => this;

        private static bool ThrowInvalidStateException()
        {
            throw new InvalidOperationException("Data dependency is in an invalid state.");
        }

        /// <inheritdoc />
        public virtual bool Add(DataFlowNode<TContents> item)
        {
            switch (_listObject)
            {
                case null:
                    _listObject = item;
                    return true;

                case DataFlowNode<TContents> node:
                    _listObject = new HashSet<DataFlowNode<TContents>>
                    {
                        node,
                        item
                    };
                    return node != item;

                case ISet<DataFlowNode<TContents>> nodes:
                    return nodes.Add(item);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<DataFlowNode<TContents>> other)
        {
            foreach (var node in other)
                Remove(node);
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
        public bool IsProperSubsetOf(IEnumerable<DataFlowNode<TContents>> other)
        {
            switch (_listObject)
            {
                case null:
                    return other.Any();
                
                case DataFlowNode<TContents> node:
                    bool containsElement = false;
                    foreach (var item in other)
                    {
                        if (node == item)
                            containsElement = true;
                        else if (containsElement)
                            return true;
                    }

                    return false;
                    
                case ISet<DataFlowNode<TContents>> nodes:
                    return nodes.IsProperSubsetOf(other);
                
                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<DataFlowNode<TContents>> other) => _listObject switch
        {
            null => false,
            DataFlowNode<TContents> _ => !other.Any(),
            ISet<DataFlowNode<TContents>> nodes => nodes.IsProperSupersetOf(other),
            _ => ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<DataFlowNode<TContents>> other) => _listObject switch
        {
            null => true,
            DataFlowNode<TContents> node => other.Contains(node),
            ISet<DataFlowNode<TContents>> nodes => nodes.IsSubsetOf(other),
            _ =>  ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<DataFlowNode<TContents>> other)
        {
            switch (_listObject)
            {
                case null:
                    return !other.Any();

                case DataFlowNode<TContents> node:
                {
                    using var enumerator = other.GetEnumerator();
                    return !enumerator.MoveNext() || enumerator.Current == node && !enumerator.MoveNext();
                }

                case ISet<DataFlowNode<TContents>> nodes:
                    return nodes.IsSupersetOf(other);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<DataFlowNode<TContents>> other) => _listObject switch
        {
            null => false,
            DataFlowNode<TContents> node => other.Contains(node),
            ISet<DataFlowNode<TContents>> nodes => nodes.Overlaps(other),
            _ => ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<DataFlowNode<TContents>> other)
        {
            switch (_listObject)
            {
                case null:
                    return !other.Any();
                
                case DataFlowNode<TContents> node:
                {
                    using var enumerator = other.GetEnumerator();
                    return enumerator.MoveNext() && node == enumerator.Current && !enumerator.MoveNext();
                }

                case ISet<DataFlowNode<TContents>> nodes:
                    return nodes.SetEquals(other);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<DataFlowNode<TContents>> other)
        {
            foreach (var item in other)
            {
                if (!Contains(item))
                    Add(item);
            }
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<DataFlowNode<TContents>> other)
        {
            foreach (var node in other)
                Add(node);
        }

        void ICollection<DataFlowNode<TContents>>.Add(DataFlowNode<TContents> item) => Add(item);

        /// <inheritdoc />
        public void Clear()
        {          
            switch (_listObject)
            {
                case null:
                    break;
                
                case DataFlowNode<TContents> node:
                    Remove(node);
                    break;
                
                case ICollection<DataFlowNode<TContents>> nodes:
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
        public bool Contains(DataFlowNode<TContents> item) => _listObject switch
        {
            null => false,
            DataFlowNode<TContents> node => item == node,
            ICollection<DataFlowNode<TContents>> nodes => nodes.Contains(item),
            _ => ThrowInvalidStateException()
        };

        /// <inheritdoc />
        public void CopyTo(DataFlowNode<TContents>[] array, int arrayIndex)
        {
            switch (_listObject)
            {
                case null:
                    break;
                
                case DataFlowNode<TContents> node:
                    array[arrayIndex] = node;
                    break;
                
                case ICollection<DataFlowNode<TContents>> nodes:
                    nodes.CopyTo(array, arrayIndex);
                    break;
                
                default:
                    ThrowInvalidStateException();
                    break;
            }
        }

        /// <inheritdoc />
        public virtual bool Remove(DataFlowNode<TContents> item) 
        {
            switch (_listObject)
            {
                case null:
                    return false;

                case DataFlowNode<TContents> node:
                    if (node == item)
                    {
                        _listObject = null;
                        return true;
                    }

                    return false;

                case ICollection<DataFlowNode<TContents>> nodes:
                    return nodes.Remove(item);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public override string ToString() => _listObject switch
        {
            null => "?",
            DataFlowNode<TContents> node => node.ToString(),
            IEnumerable<DataFlowNode<TContents>> collection => $"({string.Join(" | ", collection)})",
            _ => ThrowInvalidStateException().ToString()
        };

        /// <summary>
        /// Returns an enumerator that iterates over all data sources the data dependency defines.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<DataFlowNode<TContents>> IEnumerable<DataFlowNode<TContents>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Provides a mechanism for enumerating all data sources within a single data dependency. 
        /// </summary>
        public struct Enumerator : IEnumerator<DataFlowNode<TContents>>
        {
            private readonly DataDependencyBase<TContents> _collection;
            private HashSet<DataFlowNode<TContents>>.Enumerator _setEnumerator;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> structure.
            /// </summary>
            /// <param name="collection">The data dependency to enumerate the data sources for.</param>
            public Enumerator(DataDependencyBase<TContents> collection)
            {
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                if (collection._listObject is HashSet<DataFlowNode<TContents>> nodes)
                    _setEnumerator = nodes.GetEnumerator();
                Current = null;
            }

            /// <inheritdoc />
            public DataFlowNode<TContents> Current
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
                    
                    case DataFlowNode<TContents> node:
                        if (Current is null)
                        {
                            Current = node;
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