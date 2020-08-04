using System;

namespace Echo.DataFlow
{
    public class DataSource<TContents>
    {
        public DataSource(DataFlowNode<TContents> node)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            SlotIndex = 0;
        }

        public DataSource(DataFlowNode<TContents> node, int slotIndex)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            SlotIndex = slotIndex;
        }
        
        public DataFlowNode<TContents> Node
        {
            get;
        }
        
        public int SlotIndex
        {
            get;
        }

        protected bool Equals(DataSource<TContents> other)
        {
            return Equals(Node, other.Node) && SlotIndex == other.SlotIndex;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((DataSource<TContents>) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Node != null ? Node.GetHashCode() : 0) * 397) ^ SlotIndex;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Node)}: {Node}, {nameof(SlotIndex)}: {SlotIndex}";
        }
        
    }
}