namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for expressions in the AST
    /// </summary>
    public abstract class ExpressionBase<TInstruction> : NodeBase<TInstruction>
    {
        /// <inheritdoc />
        protected ExpressionBase(long id)
            : base(id) { }
    }
}