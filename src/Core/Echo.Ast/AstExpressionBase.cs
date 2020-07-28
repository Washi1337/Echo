namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for expressions in the AST
    /// </summary>
    public abstract class AstExpressionBase<TInstruction> : AstNodeBase<TInstruction>
    {
        /// <inheritdoc />
        protected AstExpressionBase(long id)
            : base(id) { }
    }
}