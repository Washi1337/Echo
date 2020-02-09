using System.Collections.Generic;
using System.Linq;
using Echo.DataFlow.Collections;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a data dependency of a node in a data flow graph, with one or more data sources.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in a data flow node.</typeparam>
    public class DataDependency<TContents> : IDataDependency
    {
        /// <summary>
        /// Creates a new data dependency with no data sources.
        /// </summary>
        public DataDependency()
            : this (Enumerable.Empty<DataFlowNode<TContents>>())
        {
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources.</param>
        public DataDependency(params DataFlowNode<TContents>[] dataSources)
            : this(dataSources.AsEnumerable())
        {
        }
        
        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources.</param>
        public DataDependency(IEnumerable<DataFlowNode<TContents>> dataSources)
        {
            DataSources = new DataSourceCollection<TContents>(this, dataSources);
        }

        /// <summary>
        /// Gets the node that owns the dependency.
        /// </summary>
        public DataFlowNode<TContents> Dependant
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a value indicating whether the data dependency has any known data sources. 
        /// </summary>
        public bool IsKnown => DataSources.Count > 0;

        /// <summary>
        /// Gets a collection of data sources this data dependency might pull data from.
        /// </summary>
        public DataSourceCollection<TContents> DataSources
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string dataSourcesString = IsKnown ? string.Join(" | ", DataSources) : "?";
            return $"{dataSourcesString})";
        }

        IEnumerable<IDataFlowNode> IDataDependency.GetDataSources() => DataSources;

    }
}