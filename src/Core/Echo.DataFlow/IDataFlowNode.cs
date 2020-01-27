using System.Collections.Generic;
using Echo.Core.Graphing;
using Echo.DataFlow.Values;

namespace Echo.DataFlow
{
    /// <summary>
    /// Provides members for describing a single node in a data flow graph.
    /// </summary>
    public interface IDataFlowNode : INode
    {
        /// <summary>
        /// Gets all symbolic stack values that this node depends on.
        /// </summary>
        /// <returns>The values.</returns>
        IEnumerable<ISymbolicValue> GetStackDependencies();
    }
}