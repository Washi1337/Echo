using System.Collections.Generic;
using System.Linq;
using Echo.Core.Values;

namespace Echo.DataFlow.Values
{
    /// <summary>
    /// Represents a symbolic value that resides in memory. 
    /// </summary>
    public class SymbolicValue<T> : DataDependencyBase<T>, IValue
    {
        /// <summary>
        /// Creates a new symbolic value with no data sources.
        /// </summary>
        public SymbolicValue()
        {
        }

        /// <summary>
        /// Creates a new symbolic value with a single data source.
        /// </summary>
        /// <param name="dataSource">The data source of the symbolic value.</param>
        public SymbolicValue(DataFlowNode<T> dataSource)
            : base(dataSource)
        {
        }

        /// <summary>
        /// Creates a new symbolic value with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources of the symbolic value.</param>
        public SymbolicValue(params DataFlowNode<T>[] dataSources)
            : base(dataSources)
        {
        }
        
        /// <summary>
        /// Creates a new symbolic value with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources of the symbolic value.</param>
        public SymbolicValue(IEnumerable<DataFlowNode<T>> dataSources)
            : base(dataSources.AsEnumerable())
        {
        }

        /// <inheritdoc />
        public bool IsKnown => HasKnownDataSources;

        /// <inheritdoc />
        public int Size => 0;

        /// <summary>
        /// Creates an exact copy of the value.
        /// </summary>
        /// <returns>The copied value.</returns>
        public SymbolicValue<T> Copy() => new SymbolicValue<T>(this);

        IValue IValue.Copy() => Copy();

        /// <summary>
        /// Pulls data sources from another symbolic value into the current symbolic value. 
        /// </summary>
        /// <param name="other">The other symbolic value.</param>
        /// <returns><c>true</c> if there were new data sources introduced to this symbolic value, <c>false</c> otherwise.</returns>
        public bool MergeWith(SymbolicValue<T> other)
        {
            bool changed = false;
            foreach (var source in other)
                changed |= Add(source);
            return changed;
        }
    }
}