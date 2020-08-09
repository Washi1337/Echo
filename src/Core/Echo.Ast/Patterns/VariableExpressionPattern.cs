namespace Echo.Ast.Patterns
{
    /// <summary>
    /// Describes a pattern that matches on <see cref="AstVariableExpression{TInstruction}"/>.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that is stored in the expression.</typeparam>
    public class VariableExpressionPattern<TInstruction> : ExpressionPattern<TInstruction>
    {
        /// <inheritdoc />
        protected override void MatchChildren(AstExpressionBase<TInstruction> input, MatchResult result)
        {
            result.IsSuccess = input is AstVariableExpression<TInstruction>;
        }

        /// <inheritdoc />
        public override string ToString() => "var";
    }
}