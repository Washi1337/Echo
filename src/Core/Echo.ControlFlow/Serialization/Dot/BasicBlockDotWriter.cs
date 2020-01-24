using System.IO;
using Echo.ControlFlow.Specialized;
using Echo.ControlFlow.Specialized.Blocks;
using Echo.Core.Code;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Provides an extension to the default dot writer class, that formats each node as a basic block and includes each
    /// instruction stored in the basic block in the label of the corresponding node.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that are stored in each basic block.</typeparam>
    public class BasicBlockDotWriter<TInstruction> : DotWriter
    {
        /// <inheritdoc />
        public BasicBlockDotWriter(TextWriter writer) 
            : base(writer)
        {
        }

        /// <inheritdoc />
        protected override void Write(INode node, string identifier)
        {
            WriteIdentifier(identifier);
            
            Writer.Write(" [shape=box3d, label=");
            string code = string.Join("\\l", ((Node<BasicBlock<TInstruction>>) node).Contents.Instructions) + "\\l";
            WriteIdentifier(code);
            Writer.Write(']');
            WriteSemicolon();
            Writer.WriteLine();
        }
        
    }
}