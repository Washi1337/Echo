using System.Collections.Generic;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that adds the string representation of the embedded instructions to a node in a graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the nodes contain.</typeparam>
    public class ControlFlowNodeAdorner<TInstruction> : IDotNodeAdorner
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
            if (node is ControlFlowNode<TInstruction> cfgNode)
            {
                string code = string.Join("\\l", cfgNode.Contents.Instructions) +"\\l";
                var result = new Dictionary<string, string>
                {
                    ["shape"] = NodeShape, 
                    ["label"] = code
                };
                return result;
            }

            return null;
        }
    }
}