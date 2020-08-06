using System;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents an external data source in a data flow graph. 
    /// </summary>
    /// <typeparam name="TContents">The type of contents to store in the node.</typeparam>
    public class ExternalDataSourceNode<TContents> : DataFlowNode<TContents>
    {
        /// <summary>
        /// Creates a new external data source.
        /// </summary>
        /// <param name="id">The unique identifier of the data source. This should be a negative number.</param>
        /// <param name="name">The display name of the external data source.</param>
        public ExternalDataSourceNode(long id, string name) 
            : this(id, name, default)
        {
        }

        /// <summary>
        /// Creates a new external data source.
        /// </summary>
        /// <param name="id">The unique identifier of the data source. This should be a negative number.</param>
        /// <param name="name">The display name of the external data source.</param>
        /// <param name="contents">The contents of the data flow node.</param>
        public ExternalDataSourceNode(long id, string name, TContents contents) 
            : base(id, contents)
        {
            if (id >= 0)
                throw new ArgumentException("Identifiers of external data sources should be negative.");
            Name = name;
        }

        /// <summary>
        /// Gets the name of the auxiliary data flow node.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <inheritdoc />
        public override bool IsExternal => true;

        /// <inheritdoc />
        public override string ToString() => Name;
    }
}