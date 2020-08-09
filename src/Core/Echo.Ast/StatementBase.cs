namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for statements in the AST
    /// </summary>
    public abstract class StatementBase<TInstruction> : NodeBase<TInstruction>
    {
        /// <inheritdoc />
        protected StatementBase(long id)
            : base(id) { }
    }
}