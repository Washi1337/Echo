using System.CodeDom.Compiler;
using System.IO;
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
        where TInstruction : notnull
    {
        return new AstFormatter<TInstruction>(self);
    }
}

/// <summary>
/// Provides a mechanism for stringifying AST nodes.
/// </summary>
/// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
public class AstFormatter<TInstruction> :
    IAstNodeVisitor<TInstruction, IndentedTextWriter>,
    IInstructionFormatter<Statement<TInstruction>>
    where TInstruction : notnull
{
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
    public IInstructionFormatter<TInstruction> InstructionFormatter
    {
        get;
    }

    string IInstructionFormatter<Statement<TInstruction>>.Format(in Statement<TInstruction> instruction) => Format(instruction);

    /// <summary>
    /// Formats a single AST node into a string.
    /// </summary>
    /// <param name="node">The node to format.</param>
    /// <returns>The formatted AST node.</returns>
    public string Format(in AstNode<TInstruction> node)
    {
        var writer = new StringWriter();
        node.Accept(this, new IndentedTextWriter(writer));
        return writer.ToString();
    }
    
    /// <inheritdoc />
    public void Visit(CompilationUnit<TInstruction> unit, IndentedTextWriter state) => unit.Root.Accept(this, state);

    /// <inheritdoc />
    public void Visit(AssignmentStatement<TInstruction> statement, IndentedTextWriter state)
    {
        for (int i = 0; i < statement.Variables.Count; i++)
        {
            if (i > 0)
                state.Write(", ");

            state.Write(statement.Variables[i].Name);
        }

        state.Write(" = ");
        statement.Expression.Accept(this, state);
    }

    /// <inheritdoc />
    public void Visit(ExpressionStatement<TInstruction> statement, IndentedTextWriter state)
    {
        statement.Expression.Accept(this, state);
        state.Write(';');
    }

    /// <inheritdoc />
    public void Visit(PhiStatement<TInstruction> statement, IndentedTextWriter state)
    {
        state.Write(statement.Representative.Name);
        state.Write(" = φ(");
        for (int i = 0; i < statement.Sources.Count; i++)
        {
            if (i > 0)
                state.Write(", ");
            statement.Sources[i].Accept(this, state);
        }

        state.Write(");");
    }

    /// <inheritdoc />
    public void Visit(BlockStatement<TInstruction> statement, IndentedTextWriter state)
    {
        state.WriteLine("{");
        state.Indent++;
        
        for (int i = 0; i < statement.Statements.Count; i++)
        {
            statement.Statements[i].Accept(this, state);
            state.WriteLine();
        }

        state.Indent--;
        state.Write("}");
    }

    /// <inheritdoc />
    public void Visit(ExceptionHandlerStatement<TInstruction> statement, IndentedTextWriter state)
    {
        state.WriteLine("try");
        statement.ProtectedBlock.Accept(this, state);
        state.WriteLine();

        for (int i = 0; i < statement.Handlers.Count; i++)
        {
            if (i > 0)
                state.WriteLine();
            
            statement.Handlers[i].Accept(this, state);
        }
    }

    /// <inheritdoc />
    public void Visit(HandlerClause<TInstruction> clause, IndentedTextWriter state)
    {
        state.WriteLine("handler");
        state.WriteLine('{');
        state.Indent++;

        if (clause.Prologue is not null)
        {
            state.WriteLine("prologue");
            clause.Prologue.Accept(this, state);
            state.WriteLine();
        }
        
        state.WriteLine("code");
        clause.Contents.Accept(this, state);
        state.WriteLine();

        if (clause.Epilogue is not null)
        {
            state.WriteLine("epilogue");
            clause.Epilogue.Accept(this, state);
            state.WriteLine();
        }

        state.Indent--;
        state.Write('}');
    }

    /// <inheritdoc />
    public void Visit(InstructionExpression<TInstruction> expression, IndentedTextWriter state)
    {
        state.Write(InstructionFormatter.Format(expression.Instruction));
        state.Write("(");
        
        for (int i = 0; i < expression.Arguments.Count; i++)
        {
            if (i > 0)
                state.Write(", ");
            expression.Arguments[i].Accept(this, state);
        }

        state.Write(")");
    }

    /// <inheritdoc />
    public void Visit(VariableExpression<TInstruction> expression, IndentedTextWriter state)
    {
        state.Write(expression.Variable.Name);
    }
}