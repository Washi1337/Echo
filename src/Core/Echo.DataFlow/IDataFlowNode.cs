using System.Collections.Generic;
using Echo.Core.Graphing;
using Echo.DataFlow.Values;

namespace Echo.DataFlow
{
    public interface IDataFlowNode : INode
    {
        IEnumerable<ISymbolicValue> GetStackDependencies();
    }
}