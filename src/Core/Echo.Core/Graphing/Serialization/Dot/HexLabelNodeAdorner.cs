using System.Collections.Generic;

namespace Echo.Core.Graphing.Serialization.Dot
{
    /// <summary>
    /// Represents a node adorner that adds a label to a node containing the hexadecimal representation of the
    /// node's identifier.
    /// </summary>
    public class HexLabelNodeAdorner : IDotNodeAdorner
    {
        /// <summary>
        /// Gets or sets the string to prepend to the identifier of the node.  
        /// </summary>
        public string Prefix
        {
            get;
            set;
        } = "0x";

        /// <summary>
        /// Gets or sets the string to append to the identifier of the node.
        /// </summary>
        public string Suffix
        {
            get;
            set;
        } = null;

        /// <summary>
        /// Gets or sets the minimal number of digits to use in the label.
        /// </summary>
        public int PaddingZeroes
        {
            get;
            set;
        } = 0;

        /// <inheritdoc />
        public IDictionary<string, string> GetNodeAttributes(INode node, long id) => new Dictionary<string, string>
        {
            ["label"] = $"{Prefix}{id.ToString($"X{PaddingZeroes.ToString()}")}{Suffix}"
        };
    }
}
