using System.Collections.Generic;
using System.Linq;
using Echo.Code;

namespace Echo.Ast
{
    /// <summary>
    /// Provides factory methods for creating new statements.
    /// </summary>
    public static class Statement
    {
        /// <summary>
        /// Wraps an expression into an expression statement.
        /// </summary>
        /// <param name="expression">The expression to wrap.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the expression.</typeparam>
        /// <returns>The resulting statement.</returns>
        public static ExpressionStatement<TInstruction> Expression<TInstruction>(Expression<TInstruction> expression) 
            where TInstruction : notnull
            => new(expression);

        /// <summary>
        /// Constructs a new assignment statement that assigns a value to a single variable.
        /// </summary>
        /// <param name="variable">The variable to assign the value to.</param>
        /// <param name="value">The expression representing the value.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the expression.</typeparam>
        /// <returns>The resulting statement.</returns>
        public static AssignmentStatement<TInstruction> Assignment<TInstruction>(
            IVariable variable,
            Expression<TInstruction> value)
            where TInstruction : notnull
        {
            return new AssignmentStatement<TInstruction>(variable, value);
        }
        
        /// <summary>
        /// Constructs a new assignment statement that assigns a value to an ordered list of variables.
        /// </summary>
        /// <param name="variable">The variables to assign the value to.</param>
        /// <param name="value">The expression representing the value.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the expression.</typeparam>
        /// <returns>The resulting statement.</returns>
        public static AssignmentStatement<TInstruction> Assignment<TInstruction>(
            IEnumerable<IVariable> variable,
            Expression<TInstruction> value)
            where TInstruction : notnull
        {
            return new AssignmentStatement<TInstruction>(variable, value);
        }

        /// <summary>
        /// Constructs a new Phi statement that assigns a representative variable to a set of sources. 
        /// </summary>
        /// <param name="representative">The representative variable.</param>
        /// <param name="sources">The sources.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the expression.</typeparam>
        /// <returns>The resulting statement.</returns>
        public static PhiStatement<TInstruction> Phi<TInstruction>(IVariable representative, params IVariable[] sources)
            where TInstruction : notnull
        {
            return new PhiStatement<TInstruction>(representative, sources.Select(x => x.ToExpression<TInstruction>()));
        }

        /// <summary>
        /// Constructs a new Phi statement that assigns a representative variable to a set of sources. 
        /// </summary>
        /// <param name="representative">The representative variable.</param>
        /// <param name="sources">The sources.</param>
        /// <typeparam name="TInstruction">The type of instructions stored in the expression.</typeparam>
        /// <returns>The resulting statement.</returns>
        public static PhiStatement<TInstruction> Phi<TInstruction>(
            IVariable representative, 
            params VariableExpression<TInstruction>[] sources)
            where TInstruction : notnull
        {
            return new PhiStatement<TInstruction>(representative, sources);
        }
    }

    /// <summary>
    /// Provides a base contract for statements in the AST
    /// </summary>
    public abstract class Statement<TInstruction> : AstNode<TInstruction>
        where TInstruction : notnull
    {
    }
}