using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a data dependency of a node in a data flow graph, which is a set of one or more data flow nodes where
    /// the owner node might pull data from.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in a data flow node.</typeparam>
    public abstract class DataDependency<TSource, TContents> : ISet<TSource>
        where TSource : DataSource<TContents>
    {
        private readonly List<DataFlowEdge<TContents>> _edges = new();
        private DataFlowNode<TContents> _dependent;

        /// <inheritdoc />
        public int Count => _edges.Count;

        /// <inheritdoc />
        public bool IsReadOnly => Dependent is null;

        public abstract DataDependencyType DependencyType
        {
            get;
        }

        /// <summary>
        /// Gets the node that owns the dependency.
        /// </summary>
        public DataFlowNode<TContents> Dependent
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
            if (item.Node.ParentGraph != _dependent.ParentGraph)
                throw new ArgumentException("Data source is not added to the same graph.");
            
            if (!Contains(item))
            {
                AddEdge(new(Dependent, item));
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<TSource> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<TSource> other)
        {
            foreach (var item in other)
                Add(item);
        }

        private void AddEdge(DataFlowEdge<TContents> edge)
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

        public bool Remove(DataFlowNode<TContents> node)
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

        private void RemoveEdge(DataFlowEdge<TContents> edge)
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

        public IEnumerable<DataFlowEdge<TContents>> GetEdges() => _edges;

        public IEnumerable<DataFlowNode<TContents>> GetNodes() => _edges.Select(e => e.DataSource.Node);
    }
}