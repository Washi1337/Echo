namespace Echo.DataFlow
{
    /// <summary>
    /// Represents an external data source in a data flow graph. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the node.</typeparam>
    public class ExternalDataSourceNode<TInstruction> : DataFlowNode<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new external data source.
        /// </summary>
        /// <param name="source">The external data source.</param>
        public ExternalDataSourceNode(object source) 
            : base(default)
        {
            Source = source;
        }

        /// <summary>
        /// Gets the object representing the external source of the auxiliary data flow node.
        /// </summary>
        public object Source
        {
            get;
        }

        /// <inheritdoc />
        public override bool IsExternal => true;

        /// <inheritdoc />
        public override string ToString() => Source.ToString() ?? "<External>";
    }
}