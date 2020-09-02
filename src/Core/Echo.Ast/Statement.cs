namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for statements in the AST
    /// </summary>
    public abstract class Statement<TInstruction> : AstNodeBase<TInstruction>
    {
        /// <inheritdoc />
        protected Statement(long id)
            : base(id) { }
    }
}