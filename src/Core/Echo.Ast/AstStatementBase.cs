namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for statements in the AST
    /// </summary>
    public abstract class AstStatementBase<TInstruction> : AstNodeBase<TInstruction>
    {
        /// <inheritdoc />
        protected AstStatementBase(long id)
            : base(id) { }
    }
}