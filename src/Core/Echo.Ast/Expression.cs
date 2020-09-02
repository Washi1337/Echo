namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for expressions in the AST
    /// </summary>
    public abstract class Expression<TInstruction> : AstNodeBase<TInstruction>
    {
        /// <inheritdoc />
        protected Expression(long id)
            : base(id) { }
    }
}