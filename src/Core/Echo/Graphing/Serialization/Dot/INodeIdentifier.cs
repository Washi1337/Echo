namespace Echo.Graphing.Serialization.Dot
{
    /// <summary>
    /// Provides members for obtaining unique identifiers to a node.
    /// </summary>
    public interface INodeIdentifier
    {
        /// <summary>
        /// Gets the identifier assigned to the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The identifier.</returns>
        long GetIdentifier(INode node);
    }
}