namespace Echo.Ast
{
    /// <summary>
    /// A base type for statement nodes in an AST
    /// </summary>
    public abstract class AstStatementBase : AstNodeBase
    {
        /// <inheritdoc />
        protected AstStatementBase(long id)
            : base(id) { }
    }
}