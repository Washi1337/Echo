namespace Echo.Ast
{
    /// <summary>
    /// Provides a base contract for expressions in the AST
    /// </summary>
    public abstract class Expression<TInstruction> : AstNodeBase<TInstruction> { }
}