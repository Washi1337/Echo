using System.Collections.Generic;

namespace Echo.Symbolic.Values
{
    public interface IDataSource
    {
        IList<SymbolicValue> StackDependencies
        {
            get;
        }
    }
}