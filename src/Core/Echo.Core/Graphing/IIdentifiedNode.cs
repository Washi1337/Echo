namespace Echo.Core.Graphing
{
    /// <summary>
    /// Represents a node that is tagged with an identification number.
    /// </summary>
    public interface IIdentifiedNode : INode
    {
        /// <summary>
        /// Gets the unique identifier of the node.
        /// </summary>
        long Id
        {
            get;
        }
    }
}