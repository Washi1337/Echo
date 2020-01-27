using System.Collections.Generic;
using Echo.Core.Values;

namespace Echo.DataFlow.Values
{
    /// <summary>
    /// Provides members for describing a symbolic value; a value that is not yet made concrete but can still be used for
    /// certain types of flow analysis.  
    /// </summary>
    public interface ISymbolicValue : IValue
    {
        /// <summary>
        /// Gets the node that depends on this symbolic value.
        /// </summary>
        IDataFlowNode Dependant
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of nodes that are possible data sources for this value.
        /// </summary>
        /// <returns>A collection of data source nodes.</returns>
        IEnumerable<IDataFlowNode> GetDataSources();
    }
}