namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for statements in the AST
    /// </summary>
    public abstract class AstStatement<TInstruction> : AstNodeBase<TInstruction>
    {
        /// <inheritdoc />
        protected AstStatement(long id)
            : base(id) { }
    }
}