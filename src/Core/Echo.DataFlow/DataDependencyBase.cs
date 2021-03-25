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
    public abstract class DataDependencyBase<TContents> : ISet<DataSource<TContents>>
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
        //    - DataSource<TContents>:            The dependency has a single data source.
        //    - HashSet<DataSource<TContents>>:   The dependency has multiple data sources.
        
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
        protected DataDependencyBase(DataSource<TContents> dataSource)
        {
            _listObject = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources.</param>
        protected DataDependencyBase(params DataSource<TContents>[] dataSources)
            : this(dataSources.AsEnumerable())
        {
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources.</param>
        protected DataDependencyBase(IEnumerable<DataSource<TContents>> dataSources)
        {
            _listObject = new HashSet<DataSource<TContents>>(dataSources);
        }
        
        /// <summary>
        /// Merges two data dependencies into one single dependency.
        /// </summary>
        protected DataDependencyBase(DataDependencyBase<TContents> left, DataDependencyBase<TContents> right)
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
                    var set = new HashSet<DataSource<TContents>>(left);
                    set.UnionWith(right);
                    _listObject = set;
                    break;
            }
        }
        

        /// <inheritdoc />
        public abstract bool IsReadOnly
        {
            get;
        }

        /// <inheritdoc />
        public int Count => _listObject switch
        {
            null => 0,
            DataSource<TContents> _ => 1,
            ICollection<DataSource<TContents>> collection => collection.Count,
            _ => throw new InvalidOperationException("Data dependency is in an invalid state.")
        };

        /// <summary>
        /// Gets a value indicating whether the data dependency has any known data sources. 
        /// </summary>
        public bool HasKnownDataSources => Count > 0;

        private static bool ThrowInvalidStateException()
        {
            throw new InvalidOperationException("Data dependency is in an invalid state.");
        }

        private void AssertIsWritable()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("Data dependency is in a read-only state.");
        }

        /// <inheritdoc />
        public virtual bool Add(DataSource<TContents> item)
        {
            AssertIsWritable();

            switch (_listObject)
            {
                case null:
                    _listObject = item;
                    return true;

                case DataSource<TContents> node:
                    if (node == item)
                        return false;
                    
                    _listObject = new HashSet<DataSource<TContents>>
                    {
                        node,
                        item
                    };
                    return true;

                case ISet<DataSource<TContents>> nodes:
                    return nodes.Add(item);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<DataSource<TContents>> other)
        {
            AssertIsWritable();
            
            foreach (var node in other)
                Remove(node);
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<DataSource<TContents>> other)
        {
            AssertIsWritable();
            
            var set = new HashSet<DataSource<TContents>>(other);
            foreach (var item in this)
            {
                if (!set.Contains(item))
                    Remove(item);
            }
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<DataSource<TContents>> other)
        {
            switch (_listObject)
            {
                case null:
                    return other.Any();
                
                case DataSource<TContents> node:
                    bool containsElement = false;
                    foreach (var item in other)
                    {
                        if (node == item)
                            containsElement = true;
                        else if (containsElement)
                            return true;
                    }

                    return false;
                    
                case ISet<DataSource<TContents>> nodes:
                    return nodes.IsProperSubsetOf(other);
                
                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<DataSource<TContents>> other) => _listObject switch
        {
            null => false,
            DataSource<TContents> _ => !other.Any(),
            ISet<DataSource<TContents>> nodes => nodes.IsProperSupersetOf(other),
            _ => ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<DataSource<TContents>> other) => _listObject switch
        {
            null => true,
            DataSource<TContents> node => other.Contains(node),
            ISet<DataSource<TContents>> nodes => nodes.IsSubsetOf(other),
            _ =>  ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<DataSource<TContents>> other)
        {
            switch (_listObject)
            {
                case null:
                    return !other.Any();

                case DataSource<TContents> node:
                {
                    using var enumerator = other.GetEnumerator();
                    return !enumerator.MoveNext() || enumerator.Current == node && !enumerator.MoveNext();
                }

                case ISet<DataSource<TContents>> nodes:
                    return nodes.IsSupersetOf(other);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<DataSource<TContents>> other) => _listObject switch
        {
            null => false,
            DataSource<TContents> node => other.Contains(node),
            ISet<DataSource<TContents>> nodes => nodes.Overlaps(other),
            _ => ThrowInvalidStateException(),
        };

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<DataSource<TContents>> other)
        {
            switch (_listObject)
            {
                case null:
                    return !other.Any();
                
                case DataSource<TContents> node:
                {
                    using var enumerator = other.GetEnumerator();
                    return enumerator.MoveNext() && node == enumerator.Current && !enumerator.MoveNext();
                }

                case ISet<DataSource<TContents>> nodes:
                    return nodes.SetEquals(other);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<DataSource<TContents>> other)
        {
            foreach (var item in other)
            {
                if (!Contains(item))
                    Add(item);
            }
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<DataSource<TContents>> other)
        {
            AssertIsWritable();
            foreach (var node in other)
                Add(node);
        }

        void ICollection<DataSource<TContents>>.Add(DataSource<TContents> item)
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

                case DataSource<TContents> node:
                    Remove(node);
                    break;

                case ICollection<DataSource<TContents>> nodes:
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
        public bool Contains(DataSource<TContents> item) => _listObject switch
        {
            null => false,
            DataSource<TContents> node => item == node,
            ICollection<DataSource<TContents>> nodes => nodes.Contains(item),
            _ => ThrowInvalidStateException()
        };

        /// <inheritdoc />
        public void CopyTo(DataSource<TContents>[] array, int arrayIndex)
        {
            switch (_listObject)
            {
                case null:
                    break;
                
                case DataSource<TContents> node:
                    array[arrayIndex] = node;
                    break;
                
                case ICollection<DataSource<TContents>> nodes:
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
        public bool Remove(DataFlowNode<TContents> node)
        {
            AssertIsWritable();
            
            var sourcesToRemove = new List<DataSource<TContents>>();

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
        public virtual bool Remove(DataSource<TContents> item) 
        {
            AssertIsWritable();
            
            switch (_listObject)
            {
                case null:
                    return false;

                case DataSource<TContents> node:
                    if (node == item)
                    {
                        _listObject = null;
                        return true;
                    }

                    return false;

                case ICollection<DataSource<TContents>> nodes:
                    return nodes.Remove(item);

                default:
                    return ThrowInvalidStateException();
            }
        }

        /// <summary>
        /// Gets a collection of nodes that were referenced by all data sources in this data dependency.
        /// </summary>
        public IEnumerable<DataFlowNode<TContents>> GetNodes() => this
            .Select(source => source.Node)
            .Distinct();
        
        /// <inheritdoc />
        public override string ToString() => _listObject switch
        {
            null => "?",
            DataSource<TContents> node => node.ToString(),
            IEnumerable<DataSource<TContents>> collection => $"({string.Join(" | ", collection)})",
            _ => ThrowInvalidStateException().ToString()
        };

        /// <summary>
        /// Returns an enumerator that iterates over all data sources the data dependency defines.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<DataSource<TContents>> IEnumerable<DataSource<TContents>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Provides a mechanism for enumerating all data sources within a single data dependency. 
        /// </summary>
        public struct Enumerator : IEnumerator<DataSource<TContents>>
        {
            private readonly DataDependencyBase<TContents> _collection;
            private HashSet<DataSource<TContents>>.Enumerator _setEnumerator;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> structure.
            /// </summary>
            /// <param name="collection">The data dependency to enumerate the data sources for.</param>
            public Enumerator(DataDependencyBase<TContents> collection)
            {
                _collection = collection ?? throw new ArgumentNullException(nameof(collection));
                if (collection._listObject is HashSet<DataSource<TContents>> nodes)
                    _setEnumerator = nodes.GetEnumerator();
                Current = null;
            }

            /// <inheritdoc />
            public DataSource<TContents> Current
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
                    
                    case DataSource<TContents> source:
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