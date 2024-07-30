namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a data source that refers to a stack value produced by a node in a data flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data stored in each data flow node.</typeparam>
    public class StackDataSource<TInstruction> : DataSource<TInstruction> 
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new stack data source, referencing the first stack value produced by the provided node.
        /// </summary>
        /// <param name="node">The node producing the value.</param>
        public StackDataSource(DataFlowNode<TInstruction> node)
            : base(node)
        {
        }

        /// <summary>
        /// Creates a new stack data source, referencing a stack value produced by the provided node.
        /// </summary>
        /// <param name="node">The node producing the value.</param>
        /// <param name="slotIndex">The index of the stack value that was produced by the node.</param>
        public StackDataSource(DataFlowNode<TInstruction> node, int slotIndex)
            : base(node)
        {
            SlotIndex = slotIndex;
        }
        
        /// <summary>
        /// Gets a value indicating the stack slot index that was pushed by the instruction referenced in <see cref="DataSource{TContents}.Node"/>.
        /// </summary>
        public int SlotIndex
        {
            get;
        }

        /// <inheritdoc />
        public override DataDependencyType Type => DataDependencyType.Stack;

        /// <inheritdoc />
        public override string ToString() => $"{Node.Offset:X8}#{SlotIndex}";

        /// <inheritdoc />
        protected override bool Equals(DataSource<TInstruction> other)
        {
            return base.Equals(other) && other is StackDataSource<TInstruction> source && source.SlotIndex == SlotIndex;
        }

        /// <inheritdoc />
        public override int GetHashCode() => (base.GetHashCode() * 397) ^ SlotIndex;
    }
}