using System.Collections.Generic;

namespace Echo.DataFlow.Values
{
    public interface IDataSource
    {
        IList<SymbolicValue> StackDependencies
        {
            get;
        }
    }
}