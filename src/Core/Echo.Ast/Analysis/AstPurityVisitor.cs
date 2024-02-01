using Echo.Code;

namespace Echo.Ast.Analysis;

/// <summary>
/// Provides a mechanism for traversing an AST and determining its purity.  
/// </summary>
/// <typeparam name="TInstruction">The type of instructions to store in each expression.</typeparam>
public class AstPurityVisitor<TInstruction> : IAstNodeVisitor<TInstruction, IPurityClassifier<TInstruction>, Trilean>
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="AstPurityVisitor{TInstruction}"/> class.
    /// </summary>
    public static AstPurityVisitor<TInstruction> Instance
    {
        get;
    } = new();

    /// <inheritdoc />
    public Trilean Visit(CompilationUnit<TInstruction> unit, IPurityClassifier<TInstruction> state)
    {
        return unit.Root.Accept(this, state);
    }

    /// <inheritdoc />
    public Trilean Visit(AssignmentStatement<TInstruction> statement, IPurityClassifier<TInstruction> state) => false;

    /// <inheritdoc />
    public Trilean Visit(ExpressionStatement<TInstruction> statement, IPurityClassifier<TInstruction> state)
    {
        return statement.Expression.Accept(this, state);
    }

    /// <inheritdoc />
    public Trilean Visit(PhiStatement<TInstruction> statement, IPurityClassifier<TInstruction> state) => false;

    /// <inheritdoc />
    public Trilean Visit(BlockStatement<TInstruction> statement, IPurityClassifier<TInstruction> state)
    {
        var result = Trilean.True;

        for (int i = 0; i < statement.Statements.Count && result != Trilean.False; i++)
            result &= statement.Statements[i].Accept(this, state);

        return result;
    }

    /// <inheritdoc />
    public Trilean Visit(ExceptionHandlerStatement<TInstruction> statement, IPurityClassifier<TInstruction> state)
    {
        var result = statement.ProtectedBlock.Accept(this, state);
        
        for (int i = 0; i < statement.Handlers.Count && result != Trilean.False; i++)
            result &= statement.Handlers[i].Accept(this, state);
        
        return result;
    }

    /// <inheritdoc />
    public Trilean Visit(HandlerClause<TInstruction> clause, IPurityClassifier<TInstruction> state)
    {
        var result = Trilean.True;

        if (clause.Prologue is not null)
            result &= clause.Prologue.Accept(this, state);

        if (result.ToBooleanOrFalse())
            result &= clause.Contents.Accept(this, state);
        
        if (clause.Epilogue is not null && result.ToBooleanOrFalse())
            result &= clause.Epilogue.Accept(this, state);
        
        return result;
    }

    /// <inheritdoc />
    public Trilean Visit(InstructionExpression<TInstruction> expression, IPurityClassifier<TInstruction> state)
    {
        var result = state.IsPure(expression.Instruction);

        for (int i = 0; i < expression.Arguments.Count && result != Trilean.False; i++)
            result &= expression.Arguments[i].Accept(this, state);

        return result;
    }

    /// <inheritdoc />
    public Trilean Visit(VariableExpression<TInstruction> expression, IPurityClassifier<TInstruction> state) => true;
}