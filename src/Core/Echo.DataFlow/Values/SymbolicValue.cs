using System.Collections.Generic;
using System.Linq;
using Echo.Core.Values;
using Echo.DataFlow.Collections;

namespace Echo.DataFlow.Values
{
    /// <summary>
    /// Represents a symbolic value that resides in memory. 
    /// </summary>
    public class SymbolicValue<T> : IDataDependency, IValue
    {
        /// <summary>
        /// Creates a new symbolic value with no data sources.
        /// </summary>
        public SymbolicValue()
            : this(0, Enumerable.Empty<DataFlowNode<T>>())
        {
        }

        /// <summary>
        /// Creates a new symbolic value with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources of the symbolic value.</param>
        public SymbolicValue(params DataFlowNode<T>[] dataSources)
            : this(0, dataSources.AsEnumerable())
        {
        }
        
        /// <summary>
        /// Creates a new symbolic value with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources of the symbolic value.</param>
        public SymbolicValue(IEnumerable<DataFlowNode<T>> dataSources)
            : this(0, dataSources.AsEnumerable())
        {
        }

        /// <summary>
        /// Creates a new symbolic value with no data sources.
        /// </summary>
        /// <param name="size">The size in bytes of the value at runtime.</param>
        public SymbolicValue(int size)
            : this(size, Enumerable.Empty<DataFlowNode<T>>())
        {
        }

        /// <summary>
        /// Creates a new symbolic value with the provided data sources.
        /// </summary>
        /// <param name="size">The size in bytes of the value at runtime.</param>
        /// <param name="dataSources">The data sources of the symbolic value.</param>
        public SymbolicValue(int size, params DataFlowNode<T>[] dataSources)
            : this(size, dataSources.AsEnumerable())
        {
        }
        
        /// <summary>
        /// Creates a new symbolic value with the provided data sources.
        /// </summary>
        /// <param name="size">The size in bytes of the value at runtime.</param>
        /// <param name="dataSources">The data sources of the symbolic value.</param>
        public SymbolicValue(int size, IEnumerable<DataFlowNode<T>> dataSources)
        {
            Size = size;
            DataSources = new HashSet<DataFlowNode<T>>(dataSources);
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
        public ISet<DataFlowNode<T>> DataSources
        {
            get;
        }

        IEnumerable<IDataFlowNode> IDataDependency.GetDataSources() => DataSources;

        /// <summary>
        /// Creates an exact copy of the value.
        /// </summary>
        /// <returns>The copied value.</returns>
        public SymbolicValue<T> Copy() => new SymbolicValue<T>(Size, DataSources);

        IValue IValue.Copy() => Copy();

        /// <summary>
        /// Pulls data sources from another symbolic value into the current symbolic value. 
        /// </summary>
        /// <param name="other">The other symbolic value.</param>
        /// <returns><c>true</c> if there were new data sources introduced to this symbolic value, <c>false</c> otherwise.</returns>
        public bool MergeWith(SymbolicValue<T> other)
        {
            bool changed = false;
            foreach (var source in other.DataSources)
                changed |= DataSources.Add(source);
            return changed;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            string dataSourcesString = IsKnown ? string.Join(" | ", DataSources) : "?";
            return $"{dataSourcesString} ({Size} bytes)";
        }
    }
}