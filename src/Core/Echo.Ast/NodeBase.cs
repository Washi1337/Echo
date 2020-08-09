using Echo.ControlFlow.Serialization.Dot;
using Echo.Core.Graphing;

namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for all Ast nodes
    /// </summary>
    public abstract class NodeBase<TInstruction> : TreeNodeBase
    {
        /// <summary>
        /// Assigns the unique ID to the node
        /// </summary>
        /// <param name="id">The unique identifier</param>
        protected NodeBase(long id)
            : base(id) { }

        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract void Accept<TState>(INodeVisitor<TInstruction, TState> visitor, TState state);

        /// <summary>
        /// Implements the visitor pattern
        /// </summary>
        public abstract TOut Accept<TState, TOut>(INodeVisitor<TInstruction, TState, TOut> visitor, TState state);

        internal abstract string Format(IInstructionFormatter<TInstruction> instructionFormatter);
    }
}