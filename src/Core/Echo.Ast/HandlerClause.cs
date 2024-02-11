using System.Collections.Generic;
using Echo.Graphing;

namespace Echo.Ast;

/// <summary>
/// Represents a single handler clause in an exception handler block.
/// </summary>
/// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
public class HandlerClause<TInstruction> : AstNode<TInstruction>
{
    private BlockStatement<TInstruction>? _prologue;
    private BlockStatement<TInstruction> _contents = null!;
    private BlockStatement<TInstruction>? _epilogue;

    /// <summary>
    /// Creates a new empty handler clause.
    /// </summary>
    public HandlerClause()
    {
        Contents = new BlockStatement<TInstruction>();
    }
    
    /// <summary>
    /// Gets or sets a user-defined tag further describing the handler clause.
    /// </summary>
    public object? Tag
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the prologue block that is executed before the main code of the clause, if available.
    /// </summary>
    public BlockStatement<TInstruction>? Prologue
    {
        get => _prologue;
        set => UpdateChild(ref _prologue, value);
    }

    /// <summary>
    /// Gets the main code implementing the handler.
    /// </summary>
    public BlockStatement<TInstruction> Contents
    {
        get => _contents;
        internal set => UpdateChildNotNull(ref _contents, value);
    }

    /// <summary>
    /// Gets or sets the epilogue block that is executed after the main code of the clause, if available.
    /// </summary>
    public BlockStatement<TInstruction>? Epilogue
    {
        get => _epilogue;
        set => UpdateChild(ref _epilogue, value);
    }

    /// <inheritdoc />
    public override IEnumerable<TreeNodeBase> GetChildren()
    {
        if (Prologue is not null)
            yield return Prologue;
        yield return Contents;
        if (Epilogue is not null)
            yield return Epilogue;
    }

    /// <inheritdoc />
    protected internal override void OnAttach(CompilationUnit<TInstruction> newRoot)
    {
        Prologue?.OnAttach(newRoot);
        Contents.OnAttach(newRoot);
        Epilogue?.OnAttach(newRoot);
    }

    /// <inheritdoc />
    protected internal override void OnDetach(CompilationUnit<TInstruction> oldRoot)
    {
        Prologue?.OnDetach(oldRoot);
        Contents.OnDetach(oldRoot);
        Epilogue?.OnDetach(oldRoot);
    }

    /// <inheritdoc />
    public override void Accept(IAstNodeVisitor<TInstruction> visitor) => visitor.Visit(this);

    /// <inheritdoc />
    public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
        visitor.Visit(this, state);

    /// <inheritdoc />
    public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
        visitor.Visit(this, state);
}