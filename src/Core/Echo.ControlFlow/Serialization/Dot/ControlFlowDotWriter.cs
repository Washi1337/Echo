using System.IO;
using Echo.Core.Graphing;
using Echo.Core.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Serialization.Dot
{
    /// <summary>
    /// Provides an extension to the default dot writer class, that formats each node as a basic block and includes each
    /// instruction stored in the basic block in the label of the corresponding node.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that are stored in each basic block.</typeparam>
    public class ControlFlowDotWriter<TInstruction> : DotWriter
    {
        /// <inheritdoc />
        public ControlFlowDotWriter(TextWriter writer) 
            : base(writer)
        {
        }

        /// <inheritdoc />
        protected override void Write(INode node, string identifier)
        {
            WriteIdentifier(identifier);
            
            Writer.Write(" [shape=box3d, label=");
            string code = string.Join("\\l", ((ControlFlowNode<TInstruction>) node).Contents.Instructions) + "\\l";
            WriteIdentifier(code);
            Writer.Write(']');
            WriteSemicolon();
            Writer.WriteLine();
        }

        /// <inheritdoc />
        protected override void Write(IEdge edge)
        {
            WriteIdentifier(edge.Origin.Id.ToString());
            Writer.Write(" -> ");
            WriteIdentifier(edge.Target.Id.ToString());

            if (edge is ControlFlowEdge<TInstruction> e)
            {
                Writer.Write(e.Type switch
                {
                    ControlFlowEdgeType.FallThrough => " [color=black]",
                    ControlFlowEdgeType.Conditional => " [color=red]",
                    ControlFlowEdgeType.Abnormal => " [color=gray, style=dashed]",
                    _ => string.Empty
                });
            }

            WriteSemicolon();
            Writer.WriteLine();
        }
    }
}