using System.Collections.Generic;
using Echo.Graphing;
using Echo.Graphing.Serialization.Dot;

namespace Echo.DataFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that adds the string representation of the embedded instructions to a node in a graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the nodes contain.</typeparam>
    public class DataFlowNodeAdorner<TInstruction> : IDotNodeAdorner
        where TInstruction : notnull
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
        public IDictionary<string, string>? GetNodeAttributes(INode node, long id)
        {
            switch (node)
            {
                case ExternalDataSourceNode<TInstruction> externalDataSource:
                    return new Dictionary<string, string>
                    {
                        ["shape"] = NodeShape,
                        ["label"] = externalDataSource.Source.ToString()
                    };
                
                case DataFlowNode<TInstruction> dataFlowNode:
                    return new Dictionary<string, string>
                    {
                        ["shape"] = NodeShape,
                        ["label"] = dataFlowNode.Instruction?.ToString() ?? dataFlowNode.Offset.ToString("X8")
                    };
                
                default:
                    return null;
            }
        }
    }
}