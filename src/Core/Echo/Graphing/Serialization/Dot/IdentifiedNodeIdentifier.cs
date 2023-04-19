using System;

namespace Echo.Graphing.Serialization.Dot
{
    /// <summary>
    /// Provides an implementation of the <see cref="INodeIdentifier"/> interface, that returns the value of
    /// <see cref="IIdentifiedNode.Id"/>.
    /// </summary>
    public class IdentifiedNodeIdentifier : INodeIdentifier
    {
        /// <summary>
        /// Provides a default instance of the <see cref="IdentifiedNodeIdentifier"/> class. 
        /// </summary>
        public static IdentifiedNodeIdentifier Instance
        {
            get;
        } = new();
        
        /// <inheritdoc />
        public long GetIdentifier(INode node) => node is IIdentifiedNode identifiedNode
            ? identifiedNode.Id
            : throw new ArgumentOutOfRangeException(nameof(node));
    }
}