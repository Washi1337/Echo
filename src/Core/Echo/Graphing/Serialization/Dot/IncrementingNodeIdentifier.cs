using System.Collections.Generic;

namespace Echo.Graphing.Serialization.Dot
{
    /// <summary>
    /// Provides an implementation of the <see cref="INodeIdentifier"/> interface, that maintains a counter
    /// that is increased every time a new node is assigned an identifier.
    /// </summary>
    public class IncrementingNodeIdentifier : INodeIdentifier
    {
        private readonly Dictionary<INode, long> _identifiers = new();
        
        /// <inheritdoc />
        public long GetIdentifier(INode node)
        {
            if (!_identifiers.TryGetValue(node, out long id))
            {
                id = _identifiers.Count;
                _identifiers.Add(node, id);
            }

            return id;
        }
        
    }
}