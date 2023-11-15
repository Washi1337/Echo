using Echo.Code;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for all AST nodes
    /// </summary>
    public abstract class AstNode<TInstruction> : TreeNodeBase
    {
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
        public abstract void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state);

        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state);

        internal abstract string Format(IInstructionFormatter<TInstruction> instructionFormatter);
    }
}