using System.Collections.Generic;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.DataFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that adds the string representation of the embedded instructions to a node in a graph.
    /// </summary>
    /// <typeparam name="TContents">The type of instructions the nodes contain.</typeparam>
    public class DataFlowNodeAdorner<TContents> : IDotNodeAdorner
    {
        /// <summary>
        /// Gets or sets the shape of the node.
        /// </summary>
        public string NodeShape
        {
            get;
            set;
        } = "box3d";

        /// <inheritdoc />
        public IDictionary<string, string> GetNodeAttributes(INode node)
        {
            switch (node)
            {
                case ExternalDataSource<TContents> externalDataSource:
                    return new Dictionary<string, string>
                    {
                        ["shape"] = NodeShape,
                        ["label"] = externalDataSource.Name
                    };
                
                case DataFlowNode<TContents> dataFlowNode:
                    return new Dictionary<string, string>
                    {
                        ["shape"] = NodeShape,
                        ["label"] = dataFlowNode.Contents.ToString()
                    };
                
                default:
                    return null;
            }
        }
    }
}