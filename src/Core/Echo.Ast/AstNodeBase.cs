using Echo.ControlFlow.Serialization.Dot;
using Echo.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for all Ast nodes
    /// </summary>
    public abstract class AstNodeBase<TInstruction> : TreeNodeBase
    {
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