using System.Collections.Generic;
using System.Linq;
using Echo.Core.Values;

namespace Echo.Symbolic.Values
{
    /// <summary>
    /// Represents a symbolic value that resides in memory. 
    /// </summary>
    public class SymbolicValue : IValue
    {
        public SymbolicValue()
            : this(0, Enumerable.Empty<IDataSource>())
        {
        }
        
        public SymbolicValue(int size)
            : this(size, Enumerable.Empty<IDataSource>())
        {
        }

        public SymbolicValue(int size, params IDataSource[] dataSources)
            : this(size, dataSources.AsEnumerable())
        {
        }
        
        public SymbolicValue(int size, IEnumerable<IDataSource> dataSources)
        {
            Size = size;
            DataSources = new List<IDataSource>(dataSources);
        }

        /// <inheritdoc />
        public bool IsKnown => DataSources.Count > 0;

        /// <inheritdoc />
        public int Size
        {
            get;
        }

        /// <summary>
        /// Provides a list of all data sources of this value.
        /// </summary>
        public IList<IDataSource> DataSources
        {
            get;
        }

        IValue IValue.Copy()
        {
            return Copy();
        }

        public SymbolicValue Copy()
        {
            return new SymbolicValue(Size, DataSources);
        }

        public override string ToString()
        {
            return $"{string.Join(" | ", DataSources)} ({Size} bytes)";
        }
    }
}