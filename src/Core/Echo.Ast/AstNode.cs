using System.Text;
using Echo.Code;
using Echo.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Represents a single node in an Abstract Syntax Tree (AST).
    /// </summary>
    public abstract class AstNode<TInstruction> : TreeNodeBase
    {
        /// <summary>
        /// Gets the direct parent of the AST node. 
        /// </summary>
        public new AstNode<TInstruction>? Parent => base.Parent as AstNode<TInstruction>;
        
        /// <summary>
        /// Gets or sets the original address range this AST node mapped to in the raw disassembly of the code
        /// (if available).
        /// </summary>
        public AddressRange? OriginalRange
        {
            get;
            set;
        }
        
        /// <summary>
        /// Determines whether the AST node consists of only pure expressions that do not affect state.
        /// </summary>
        /// <param name="classifier">The object responsible for classifying individual instructions for purity</param>
        /// <returns>
        /// <c>true</c> if the node is fully pure, <c>false</c> if fully impure, and <c>Unknown</c> if purity could
        /// not be determined for (parts of) the AST node.
        /// </returns>
        public Trilean IsPure(IPurityClassifier<TInstruction> classifier)
        {
            return Accept(Analysis.AstPurityVisitor<TInstruction>.Instance, classifier);
        }
        
        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract void Accept(IAstNodeVisitor<TInstruction> visitor);
        
        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state);

        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state);

        /// <inheritdoc />
        public override string ToString()
        {
            var formatter = new AstFormatter<TInstruction>();
            var builder = new StringBuilder();
            Accept(formatter, builder);
            return builder.ToString();
        }
    }
}