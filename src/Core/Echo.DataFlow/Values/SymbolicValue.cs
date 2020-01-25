using System.Collections.Generic;
using System.Linq;
using Echo.Core.Values;
using Echo.DataFlow.Collections;

namespace Echo.DataFlow.Values
{
    /// <summary>
    /// Represents a symbolic value that resides in memory. 
    /// </summary>
    public class SymbolicValue<T> : ISymbolicValue
    {
        public SymbolicValue()
            : this(0, Enumerable.Empty<DataFlowNode<T>>())
        {
        }

        public SymbolicValue(params DataFlowNode<T>[] dataSources)
            : this(0, dataSources.AsEnumerable())
        {
        }
        
        public SymbolicValue(IEnumerable<DataFlowNode<T>> dataSources)
            : this(0, dataSources.AsEnumerable())
        {
        }
        
        public SymbolicValue(int size)
            : this(size, Enumerable.Empty<DataFlowNode<T>>())
        {
        }

        public SymbolicValue(int size, params DataFlowNode<T>[] dataSources)
            : this(size, dataSources.AsEnumerable())
        {
        }
        
        public SymbolicValue(int size, IEnumerable<DataFlowNode<T>> dataSources)
        {
            Size = size;
            DataSources = new DataSourceCollection<T>(this, dataSources);
        }

        public DataFlowNode<T> Dependant
        {
            get;
            internal set;
        }

        IDataFlowNode ISymbolicValue.Dependant => Dependant;

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
        public DataSourceCollection<T> DataSources
        {
            get;
        }

        IEnumerable<IDataFlowNode> ISymbolicValue.GetDataSources() => DataSources;

        public SymbolicValue<T> Copy() => new SymbolicValue<T>(Size, DataSources);

        IValue IValue.Copy() => Copy();

        public bool MergeWith(SymbolicValue<T> other)
        {
            bool changed = false;
            foreach (var source in other.DataSources)
                changed |= DataSources.Add(source);
            return changed;
        }

        public override string ToString()
        {
            string dataSourcesString = IsKnown ? string.Join(" | ", DataSources) : "?";
            return $"{dataSourcesString} ({Size} bytes)";
        }
    }
}