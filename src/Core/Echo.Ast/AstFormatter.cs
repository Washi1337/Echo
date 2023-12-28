using System.Text;
using Echo.ControlFlow.Serialization.Dot;

namespace Echo.Ast;

/// <summary>
/// Provides extension methods for stringifying AST nodes.
/// </summary>
public static class AstFormatter
{
    /// <summary>
    /// Wraps an instruction formatter into a new AST formatter.
    /// </summary>
    /// <param name="self">The instruction formatter.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
    /// <returns>The constructed formatter.</returns>
    public static AstFormatter<TInstruction> ToAstFormatter<TInstruction>(this IInstructionFormatter<TInstruction> self)
    {
        return new AstFormatter<TInstruction>(self);
    }
}

/// <summary>
/// Provides a mechanism for stringifying AST nodes.
/// </summary>
/// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
public class AstFormatter<TInstruction> : IAstNodeVisitor<TInstruction, StringBuilder>, IInstructionFormatter<Statement<TInstruction>>
{
    /// <summary>
    /// Gets the default instance of the formatter using the default formatter for <typeparamref cref="TInstruction"/>.
    /// </summary>
    public static AstFormatter<TInstruction> Default { get; } = new();
    
    /// <summary>
    /// Creates a new AST formatter using the default instruction formatter.
    /// </summary>
    public AstFormatter()
        : this(DefaultInstructionFormatter<TInstruction>.Instance)
    {
    }
    
    /// <summary>
    /// Creates a new AST formatter using the provided instruction formatter.
    /// </summary>
    public AstFormatter(IInstructionFormatter<TInstruction> instructionFormatter)
    {
        InstructionFormatter = instructionFormatter;
    }

    /// <summary>
    /// Gets the instruction formatter used for formatting <see cref="InstructionExpression{TInstruction}"/> instances.
    /// </summary>
    public IInstructionFormatter<TInstruction> InstructionFormatter { get; }
    
    /// <inheritdoc />
    public string Format(in Statement<TInstruction> instruction)
    {
        var builder = new StringBuilder();
        instruction.Accept(this, builder);
        return builder.ToString();
    }

    /// <inheritdoc />
    public void Visit(AssignmentStatement<TInstruction> statement, StringBuilder state)
    {
        for (int i = 0; i < statement.Variables.Count; i++)
        {
            if (i > 0)
                state.Append(", ");

            state.Append(statement.Variables[i].Name);
        }

        state.Append(" = ");
        statement.Expression.Accept(this, state);
    }

    /// <inheritdoc />
    public void Visit(ExpressionStatement<TInstruction> expression, StringBuilder state)
    {
        expression.Expression.Accept(this, state);
        state.Append(';');
    }

    /// <inheritdoc />
    public void Visit(PhiStatement<TInstruction> statement, StringBuilder state)
    {
        state.Append(statement.Representative.Name);
        state.Append(" = φ(");
        for (int i = 0; i < statement.Sources.Count; i++)
        {
            if (i > 0)
                state.Append(", ");
            statement.Sources[i].Accept(this, state);
        }
        state.Append(");");
    }

    /// <inheritdoc />
    public void Visit(InstructionExpression<TInstruction> expression, StringBuilder state)
    {
        state.Append(InstructionFormatter.Format(expression.Instruction));
        state.Append("(");
        for (int i = 0; i < expression.Arguments.Count; i++)
        {
            if (i > 0)
                state.Append(", ");
            expression.Arguments[i].Accept(this, state);
        }
        state.Append(")");
    }

    /// <inheritdoc />
    public void Visit(VariableExpression<TInstruction> expression, StringBuilder state)
    {
        state.Append(expression.Variable.Name);
    }

}