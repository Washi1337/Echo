using System.Collections.Generic;
using System.Text;
using Echo.Graphing;
using Echo.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Represents an adorner that adds the string representation of the embedded instructions to a node in a graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions the nodes contain.</typeparam>
    public class ControlFlowNodeAdorner<TInstruction> : IDotNodeAdorner
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new <see cref="ControlFlowNodeAdorner{TInstruction}"/> with the default formatter.
        /// </summary>
        public ControlFlowNodeAdorner()
            : this(DefaultInstructionFormatter<TInstruction>.Instance) { }

        /// <summary>
        /// Creates a new <see cref="ControlFlowNodeAdorner{TInstruction}"/> with
        /// the specified <see cref="IInstructionFormatter{TInstruction}"/>.
        /// </summary>
        /// <param name="formatter">The <see cref="IInstructionFormatter{TInstruction}"/> to format instructions with.</param>
        public ControlFlowNodeAdorner(IInstructionFormatter<TInstruction> formatter) =>
            InstructionFormatter = formatter;
        
        /// <summary>
        /// Gets or sets the shape of the node.
        /// </summary>
        public string NodeShape
        {
            get;
            set;
        } = "box3d";

        /// <summary>
        /// Gets or sets a value indicating whether the adorner should add block headers to every node.
        /// </summary>
        public bool IncludeBlockHeaders
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the adorner should add the block instructions to every node.
        /// </summary>
        public bool IncludeInstructions
        {
            get;
            set;
        } = true;
        
        /// <summary>
        /// Gets or sets a value indicating the format of block headers. This is a format string with one
        /// parameter containing the value of <see cref="ControlFlowNode{TInstruction}.Offset"/>.
        /// </summary>
        public string BlockHeaderFormat
        {
            get;
            set;
        } = "Block_{0:X8}:";

        /// <summary>
        /// Gets or sets the formatter that will be used to format the instructions.
        /// </summary>
        public IInstructionFormatter<TInstruction> InstructionFormatter
        {
            get;
            set;
        }
        
        /// <inheritdoc />
        public IDictionary<string, string>? GetNodeAttributes(INode node, long id)
        {
            if (node is ControlFlowNode<TInstruction> cfgNode)
            {
                var contentsBuilder = new StringBuilder();

                if (IncludeBlockHeaders)
                {
                    contentsBuilder.AppendFormat(BlockHeaderFormat, cfgNode.Offset);
                    contentsBuilder.Append("\\l");
                }

                if (IncludeInstructions)
                {
                    for (int i = 0; i < cfgNode.Contents.Instructions.Count; i++)
                    {
                        var instruction = cfgNode.Contents.Instructions[i];
                        contentsBuilder.Append(InstructionFormatter.Format(instruction));
                        contentsBuilder.Append("\\l");
                    }
                }

                var result = new Dictionary<string, string>
                {
                    ["shape"] = NodeShape, 
                    ["label"] = contentsBuilder.ToString()
                };
                return result;
            }

            return null;
        }
    }
}