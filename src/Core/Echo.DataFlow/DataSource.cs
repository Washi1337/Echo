using System;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a data source in a data flow graph.
    /// </summary>
    /// <typeparam name="TContents">The type of data stored in each data flow node.</typeparam>
    public class DataSource<TContents>
    {
        /// <summary>
        /// Creates a new data source.
        /// </summary>
        /// <param name="node">The node producing the data.</param>
        public DataSource(DataFlowNode<TContents> node)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            SlotIndex = 0;
        }

        /// <summary>
        /// Creates a new stack data source.
        /// </summary>
        /// <param name="node">The node producing the data.</param>
        /// <param name="slotIndex">gets a value indicating the stack slot index that was pushed by the instruction
        /// referenced in <paramref name="node"/>.</param>
        public DataSource(DataFlowNode<TContents> node, int slotIndex)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            SlotIndex = slotIndex;
        }
        
        /// <summary>
        /// Gets the data flow node that produced the data.
        /// </summary>
        public DataFlowNode<TContents> Node
        {
            get;
        }
        
        /// <summary>
        /// When this data dependency is a stack data dependency, gets a value indicating the stack slot index that was
        /// pushed by the instruction referenced in <see cref="Node"/>.
        /// </summary>
        public int SlotIndex
        {
            get;
        }

        /// <summary>
        /// Determines whether the data sources are considered equal.
        /// </summary>
        /// <param name="other">The other data source.</param>
        protected bool Equals(DataSource<TContents> other)
        {
            return Equals(Node, other.Node) && SlotIndex == other.SlotIndex;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
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
        
        /// <summary>
        /// Determines whether the data sources are considered equal.
        /// </summary>
        /// <param name="a">The first data source.</param>
        /// <param name="b">The second data source.</param>
        /// <returns><c>true</c> if they are considered equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(DataSource<TContents> a, DataSource<TContents> b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            return a.Equals(b);
        }

        /// <summary>
        /// Determines whether the data sources are not considered equal.
        /// </summary>
        /// <param name="a">The first data source.</param>
        /// <param name="b">The second data source.</param>
        /// <returns><c>true</c> if they are not considered equal, <c>false</c> otherwise.</returns>
        public static bool operator !=(DataSource<TContents> a, DataSource<TContents> b)
        {
            return !(a == b);
        }
        
    }
}