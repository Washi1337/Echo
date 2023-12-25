using System.Collections.Generic;
using Echo.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Provides factory methods for constructing expressions.
    /// </summary>
    public static class Expression
    {
        /// <summary>
        /// Wraps the provided variable into a variable expression.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <typeparam name="TInstruction">The type of instruction.</typeparam>
        /// <returns>The resulting expression.</returns>
        public static VariableExpression<TInstruction> Variable<TInstruction>(IVariable variable) => new(variable);
        
        /// <summary>
        /// Wraps an instruction into an expression with no arguments.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <typeparam name="TInstruction">The type of instruction.</typeparam>
        /// <returns>The resulting expression.</returns>
        public static InstructionExpression<TInstruction> Instruction<TInstruction>(TInstruction instruction) => new(instruction);

        /// <summary>
        /// Wraps an instruction into an expression with the provided arguments.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arguments">The arguments.</param>
        /// <typeparam name="TInstruction">The type of instruction.</typeparam>
        /// <returns>The resulting expression.</returns>
        public static InstructionExpression<TInstruction> Instruction<TInstruction>(
            TInstruction instruction,
            params Expression<TInstruction>[] arguments)
        {
            return new InstructionExpression<TInstruction>(instruction, arguments);
        }
        
        /// <summary>
        /// Wraps an instruction into an expression with the provided arguments.
        /// </summary>
        /// <param name="instruction">The instruction.</param>
        /// <param name="arguments">The arguments.</param>
        /// <typeparam name="TInstruction">The type of instruction.</typeparam>
        /// <returns>The resulting expression.</returns>
        public static InstructionExpression<TInstruction> Instruction<TInstruction>(
            TInstruction instruction,
            IEnumerable<Expression<TInstruction>> arguments)
        {
            return new InstructionExpression<TInstruction>(instruction, arguments);
        }

        /// <summary>
        /// Wraps the provided variable into a variable expression.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <typeparam name="TInstruction">The type of instruction.</typeparam>
        /// <returns>The resulting expression.</returns>
        public static VariableExpression<TInstruction> ToExpression<TInstruction>(this IVariable variable) 
            => new(variable);
    }
    
    /// <summary>
    /// Provides a base contract for expressions in the AST
    /// </summary>
    public abstract class Expression<TInstruction> : AstNode<TInstruction>
    {
        /// <summary>
        /// Wraps the expression into an expression statement.
        /// </summary>
        /// <returns>The resulting statement.</returns>
        public ExpressionStatement<TInstruction> ToStatement() => new(this);
    }
}