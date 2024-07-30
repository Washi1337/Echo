using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Echo.DataFlow
{
    /// <summary>
    /// Provides a base for data dependencies of a node in a data flow graph, which is a set of one or more data flow
    /// nodes where the owner node might pull data from.
    /// </summary>
    /// <typeparam name="TSource">The type of data source that this dependency uses.</typeparam>
    /// <typeparam name="TInstruction">The type of contents to put in a data flow node.</typeparam>
    public abstract class DataDependency<TSource, TInstruction> : ISet<TSource>
        where TSource : DataSource<TInstruction>
        where TInstruction : notnull
    {
        private readonly List<DataFlowEdge<TInstruction>> _edges = new();
        private DataFlowNode<TInstruction>? _dependent;

        /// <inheritdoc />
        public int Count => _edges.Count;

        /// <inheritdoc />
        public bool IsReadOnly => Dependent is null;

        /// <summary>
        /// Gets a value indicating whether the data dependency has any known data sources. 
        /// </summary>
        public bool HasKnownDataSources => Count > 0;

        /// <summary>
        /// Gets the node that owns the dependency.
        /// </summary>
        public DataFlowNode<TInstruction>? Dependent
        {
            get => _dependent;
            internal set
            {          
                if (_dependent != value)
                {
                    if (_dependent is not null)
                    {
                        foreach (var edge in _edges)
                            edge.DataSource.Node.IncomingEdges.Remove(edge);
                    }

                    _dependent = value;
                }
            }
        }

        private void AssertDependentIsNotNull()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(
                    "Cannot modify data dependency when it is not assigned a dependent node.");
            }
        }

        /// <inheritdoc />
        public bool Add(TSource item)
        {
            AssertDependentIsNotNull();
            
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (_dependent is null || item.Node.ParentGraph != _dependent.ParentGraph)
                throw new ArgumentException("Data source is not added to the same graph.");
            
            if (!Contains(item))
            {
                AddEdge(new DataFlowEdge<TInstruction>(_dependent, item));
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<TSource> other)
        {
            foreach (var item in other)
                Remove(item);
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<TSource> other)
        {
            var set = new HashSet<TSource>(other);
            foreach (var edge in _edges)
            {
                if (!set.Contains(edge.DataSource))
                    RemoveEdge(edge);
            }
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<TSource> other)
        {
            var set = new HashSet<TSource>(other);
            if (_edges.Count >= set.Count)
                return false;
            
            foreach (var edge in _edges)
            {
                if (!set.Contains(edge.DataSource))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<TSource> other)
        {
            var set = new HashSet<TSource>(other);
            if (_edges.Count <= set.Count)
                return false;
            
            foreach (var source in set)
            {
                if (!Contains(source))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<TSource> other)
        {
            var set = new HashSet<TSource>(other);
            if (_edges.Count > set.Count)
                return false;
            
            foreach (var edge in _edges)
            {
                if (!set.Contains(edge.DataSource))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<TSource> other)
        {
            var set = new HashSet<TSource>(other);
            if (_edges.Count < set.Count)
                return false;
            
            foreach (var source in set)
            {
                if (!Contains(source))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<TSource> other)
        {
            foreach (var item in other)
            {
                if (Contains(item))
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<TSource> other)
        {
            var set = new HashSet<TSource>(other);
            if (set.Count != Count)
                return false;

            foreach (var edge in _edges)
            {
                if (!set.Contains(edge.DataSource))
                    return false;
            }

            return true;
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<TSource> other)
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
        public void UnionWith(IEnumerable<TSource> other)
        {
            foreach (var item in other)
                Add(item);
        }

        private void AddEdge(DataFlowEdge<TInstruction> edge)
        {
            _edges.Add(edge);
            edge.DataSource.Node.IncomingEdges.Add(edge);
        }

        /// <inheritdoc />
        void ICollection<TSource>.Add(TSource item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            AssertDependentIsNotNull();

            while (Count > 0)
                RemoveEdge(_edges[0]);
        }

        /// <inheritdoc />
        public bool Contains(TSource item)
        {
            AssertDependentIsNotNull();
            return _edges.Any(e => e.DataSource.Equals(item));
        }

        /// <inheritdoc />
        public void CopyTo(TSource[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < _edges.Count)
                throw new ArgumentException("Not enough space in target array.");

            for (int i = 0; i < _edges.Count; i++)
                array[arrayIndex + i] = (TSource) _edges[i].DataSource;
        }

        /// <inheritdoc />
        public bool Remove(TSource item)
        {
            AssertDependentIsNotNull();

            var edge = _edges.FirstOrDefault(e => e.DataSource.Equals(item));
            if (edge is null)
                return false;
            
            RemoveEdge(edge);
            return true;
        }

        /// <summary>
        /// Removes all data sources that are incident with the provided node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns><c>true</c> if at least one edge was removed, <c>false</c> otherwise.</returns>
        public bool Remove(DataFlowNode<TInstruction> node)
        {
            AssertDependentIsNotNull();

            bool changed = false;
            
            for (int i = 0; i < _edges.Count; i++)
            {
                if (_edges[i].DataSource.Node == node)
                {
                    RemoveEdge(_edges[i]);
                    i--;
                    changed = true;
                }
            }

            return changed;
        }

        private void RemoveEdge(DataFlowEdge<TInstruction> edge)
        {
            if (_edges.Remove(edge))
            {
                edge.DataSource.Node.IncomingEdges.Remove(edge);
            }
        }

        /// <inheritdoc />
        public IEnumerator<TSource> GetEnumerator() => 
            _edges.Select(e => (TSource) e.DataSource).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => 
            GetEnumerator();

        /// <summary>
        /// Gets a collection of data flow edges that encode the stored data sources.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<DataFlowEdge<TInstruction>> GetEdges() => _edges;

        /// <summary>
        /// Gets a collection of nodes that are possible data sources for the dependency.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DataFlowNode<TInstruction>> GetNodes() => _edges.Select(e => e.DataSource.Node);
    }
}