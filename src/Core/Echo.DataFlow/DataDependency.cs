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
        private DataFlowNode<TContents> _dependant;

        /// <summary>
        /// Creates a new data dependency with no data sources.
        /// </summary>
        /// <param name="dependencyType">The type of data dependency.</param>
        public DataDependency(DataDependencyType dependencyType)
            : this (dependencyType, Enumerable.Empty<DataFlowNode<TContents>>())
        {
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dependencyType">The type of data dependency.</param>
        /// <param name="dataSources">The data sources.</param>
        public DataDependency(DataDependencyType dependencyType, params DataFlowNode<TContents>[] dataSources)
            : this(dependencyType, dataSources.AsEnumerable())
        {
        }
        
        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dependencyType">The type of data dependency.</param>
        /// <param name="dataSources">The data sources.</param>
        public DataDependency(DataDependencyType dependencyType, IEnumerable<DataFlowNode<TContents>> dataSources)
        {
            DependencyType = dependencyType;
            DataSources = new DataSourceCollection<TContents>(this, dataSources);
        }

        /// <summary>
        /// Gets the type of dependency this object encodes.
        /// </summary>
        public DataDependencyType DependencyType
        {
            get;
        }

        /// <summary>
        /// Gets the node that owns the dependency.
        /// </summary>
        public DataFlowNode<TContents> Dependant
        {
            get => _dependant;
            internal set => _dependant = value;
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

        /// <summary>
        /// Constructs edges for a data flow graph, based on this data dependency.
        /// </summary>
        public IEnumerable<DataFlowEdge<TContents>> GetEdges()
        {
            return DataSources.Select(source =>
                new DataFlowEdge<TContents>(Dependant, source, DependencyType));
        }
    }
}