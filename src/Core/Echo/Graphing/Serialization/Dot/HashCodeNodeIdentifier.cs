namespace Echo.Graphing.Serialization.Dot
{
    /// <summary>
    /// Provides an implementation of the <see cref="INodeIdentifier"/> interface, that returns the hash code of the
    /// node object as unique identifiers.
    /// </summary>
    public class HashCodeNodeIdentifier : INodeIdentifier
    {
        /// <summary>
        /// Provides a default instance of the <see cref="HashCodeNodeIdentifier"/> class. 
        /// </summary>
        public static HashCodeNodeIdentifier Instance
        {
            get;
        } = new HashCodeNodeIdentifier();
        
        /// <inheritdoc />
        public long GetIdentifier(INode node) => node.GetHashCode();
    }
}