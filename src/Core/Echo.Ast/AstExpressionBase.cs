namespace Echo.Ast
{
    /// <summary>
    /// A base type for expression nodes in an AST
    /// </summary>
    public abstract class AstExpressionBase : AstNodeBase
    {
        /// <inheritdoc />
        protected AstExpressionBase(long id) 
            : base(id) { }
    }
}