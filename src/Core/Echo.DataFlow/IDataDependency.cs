using System.Collections.Generic;

namespace Echo.DataFlow
{
    /// <summary>
    /// Provides members for describing a symbolic value; a value that is not yet made concrete but can still be used for
    /// certain types of flow analysis.  
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