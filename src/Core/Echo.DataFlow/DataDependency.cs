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
    public class DataDependency<TContents> : ISet<DataSource<TContents>>
    {
        private readonly List<DataFlowEdge<TContents>> _edges = new();

        /// <summary>
        /// Creates a new data dependency with no data sources.
        /// </summary>
        public DataDependency(DataDependencyType dependencyType)
        {
            DependencyType = dependencyType;
        }

        /// <inheritdoc />
        public int Count => _edges.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        public DataDependencyType DependencyType
        {
            get;
        }

        /// <summary>
        /// Gets the node that owns the dependency.
        /// </summary>
        public DataFlowNode<TContents> Dependent
        {
            get;
            internal set;
        }

        private void AssertDependentIsNotNull()
        {
            if (Dependent is null)
            {
                throw new InvalidOperationException(
                    "Cannot modify data dependency when it is not assigned a dependent node.");
            }
        }

        /// <inheritdoc />
        public bool Add(DataSource<TContents> item)
        {
            AssertDependentIsNotNull();
            
            if (!Contains(item))
            {
                AddEdge(CreateEdge(item));
                return true;
            }

            return false;
        }

        private DataFlowEdge<TContents> CreateEdge(DataSource<TContents> item)
        {
            return new(Dependent, item, DependencyType);
        }

        /// <inheritdoc />
        public void ExceptWith(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void IntersectWith(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsProperSubsetOf(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsProperSupersetOf(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSubsetOf(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool IsSupersetOf(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Overlaps(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool SetEquals(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SymmetricExceptWith(IEnumerable<DataSource<TContents>> other)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UnionWith(IEnumerable<DataSource<TContents>> other)
        {
            foreach (var item in other)
                Add(item);
        }

        private void AddEdge(DataFlowEdge<TContents> edge)
        {
            _edges.Add(edge);
        }

        /// <inheritdoc />
        void ICollection<DataSource<TContents>>.Add(DataSource<TContents> item) => Add(item);
        
        /// <inheritdoc />
        public void Clear()
        {
            AssertDependentIsNotNull();

            while (Count > 0)
                RemoveEdge(_edges[0]);
        }

        /// <inheritdoc />
        public bool Contains(DataSource<TContents> item)
        {
            AssertDependentIsNotNull();
            return _edges.Any(e => e.DataSource.Equals(item));
        }

        /// <inheritdoc />
        public void CopyTo(DataSource<TContents>[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Length - arrayIndex < _edges.Count)
                throw new ArgumentException("Not enough space in target array.");

            for (int i = 0; i < _edges.Count; i++)
                array[arrayIndex + i] = _edges[i].DataSource;
        }

        /// <inheritdoc />
        public bool Remove(DataSource<TContents> item)
        {
            AssertDependentIsNotNull();

            var edge = _edges.FirstOrDefault(e => e.DataSource.Equals(item));
            if (edge is null)
                return false;
            
            RemoveEdge(edge);
            return true;
        }

        private void RemoveEdge(DataFlowEdge<TContents> edge)
        {
            _edges.Remove(edge);
        }

        /// <inheritdoc />
        public IEnumerator<DataSource<TContents>> GetEnumerator() => 
            _edges.Select(e => e.DataSource).GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => 
            GetEnumerator();
    }
}