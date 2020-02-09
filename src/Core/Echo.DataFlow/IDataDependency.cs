using System.Collections.Generic;

namespace Echo.DataFlow
{
    /// <summary>
    /// Provides members for describing a dependency of a node in a data flow graph.
    /// </summary>
    public interface IDataDependency
    {
        /// <summary>
        /// Gets a collection of nodes that are possible data sources for this value.
        /// </summary>
        /// <returns>A collection of data source nodes.</returns>
        IEnumerable<IDataFlowNode> GetDataSources();
    }
}