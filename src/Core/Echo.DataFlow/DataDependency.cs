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
    public class DataDependency<TContents> : ICollection<DataSource<TContents>>
    {
        private DataFlowNode<TContents> _dependant;

        /// <summary>
        /// Creates a new data dependency with no data sources.
        /// </summary>
        public DataDependency()
        {
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        public DataDependency(DataFlowNode<TContents> sourceNode)
        {
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        public DataDependency(DataSource<TContents> dataSource)
        {
        }
        
        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="sourceNodes">The data sources.</param>
        public DataDependency(IEnumerable<DataFlowNode<TContents>> sourceNodes)
        {
        }
        
        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources.</param>
        public DataDependency(IEnumerable<DataSource<TContents>> dataSources)
        {
        }

        /// <inheritdoc />
        public int Count
        {
            get;
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the node that owns the dependency.
        /// </summary>
        public DataFlowNode<TContents> Dependant
        {
            get => _dependant;
            internal set
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc />
        public bool Add(DataSource<TContents> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Contains(DataSource<TContents> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void CopyTo(DataSource<TContents>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        void ICollection<DataSource<TContents>>.Add(DataSource<TContents> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Remove(DataSource<TContents> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IEnumerator<DataSource<TContents>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}