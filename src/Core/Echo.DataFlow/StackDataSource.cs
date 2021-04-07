namespace Echo.DataFlow
{
    public class StackDataSource<TContents> : DataSource<TContents>
    {
        public StackDataSource(DataFlowNode<TContents> node)
            : base(node)
        {
        }

        public StackDataSource(DataFlowNode<TContents> node, int slotIndex)
            : base(node)
        {
            SlotIndex = slotIndex;
        }
        
        /// <summary>
        /// Gets a value indicating the stack slot index that was pushed by the instruction referenced in <see cref="Node"/>.
        /// </summary>
        public int SlotIndex
        {
            get;
        }

        /// <inheritdoc />
        public override DataDependencyType Type => DataDependencyType.Stack;

        /// <inheritdoc />
        public override string ToString() => $"{Node.Id:X8}#{SlotIndex}";

        /// <inheritdoc />
        protected override bool Equals(DataSource<TContents> other)
        {
            return base.Equals(other) && other is StackDataSource<TContents> source && source.SlotIndex == SlotIndex;
        }

        /// <inheritdoc />
        public override int GetHashCode() => (base.GetHashCode() * 397) ^ SlotIndex;
    }
}