using System.Collections.Generic;
using Echo.Core.Values;

namespace Echo.DataFlow.Values
{
    public interface ISymbolicValue : IValue
    {
        IDataFlowNode Dependant
        {
            get;
        }
        
        IEnumerable<IDataFlowNode> GetDataSources();
    }
}