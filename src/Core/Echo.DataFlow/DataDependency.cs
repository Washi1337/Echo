using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a data dependency of a node in a data flow graph, which is a set of one or more data flow nodes where
    /// the owner node might pull data from.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in a data flow node.</typeparam>
    public class DataDependency<TContents> : DataDependencyBase<TContents>
    {
        private DataFlowNode<TContents> _dependant;

        /// <summary>
        /// Creates a new data dependency with no data sources.
        /// </summary>
        public DataDependency()
        {
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        public DataDependency(DataFlowNode<TContents> sourceNode)
            : base(new DataSource<TContents>(sourceNode))
        {
        }

        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        public DataDependency(DataSource<TContents> dataSource)
            : base(dataSource)
        {
        }
        
        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="sourceNodes">The data sources.</param>
        public DataDependency(IEnumerable<DataFlowNode<TContents>> sourceNodes)
            : base(sourceNodes.Select(source => new DataSource<TContents>(source)))
        {
        }
        
        /// <summary>
        /// Creates a new data dependency with the provided data sources.
        /// </summary>
        /// <param name="dataSources">The data sources.</param>
        public DataDependency(IEnumerable<DataSource<TContents>> dataSources)
            : base(dataSources)
        {
        }
        
        /// <summary>
        /// Gets the node that owns the dependency.
        /// </summary>
        public DataFlowNode<TContents> Dependant
        {
            get => _dependant;
            internal set
            {
                if (_dependant != value)
                {
                    if (_dependant != null)
                    {
                        foreach (var source in this)
                            source.Node.Dependants.Remove(_dependant);
                    }

                    _dependant = value;
                    
                    if (_dependant != null)
                    {
                        foreach (var source in this)
                            source.Node.Dependants.Add(_dependant);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override bool Add(DataSource<TContents> item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (Dependant != null && item.Node.ParentGraph != Dependant.ParentGraph)
                throw new ArgumentException("Data source is not added to the same graph.");

            if (base.Add(item))
            {
                item.Node.Dependants.Add(Dependant);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Remove(DataSource<TContents> item)
        {
            if (item is null)
                return false;
            
            if (base.Remove(item))
            {
                item.Node.Dependants.Remove(Dependant);
                return true;
            }

            return false;
        }
        
    }
}