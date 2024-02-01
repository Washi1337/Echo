using System.Collections.Generic;
using Echo.Graphing;

namespace Echo.Ast;

/// <summary>
/// Represents a single exception handler block statement in an AST.
/// </summary>
/// <typeparam name="TInstruction">The type of instruction stored in the AST.</typeparam>
public class ExceptionHandlerStatement<TInstruction> : Statement<TInstruction>
{
    private BlockStatement<TInstruction> _protectedBlock = null!;

    /// <summary>
    /// Creates a new empty exception handler statement.
    /// </summary>
    public ExceptionHandlerStatement()
    {
        ProtectedBlock = new BlockStatement<TInstruction>();
        Handlers = new TreeNodeCollection<ExceptionHandlerStatement<TInstruction>, HandlerClause<TInstruction>>(this);
    }
    
    /// <summary>
    /// Gets the block of code that is protected by the exception handler.
    /// </summary>
    public BlockStatement<TInstruction> ProtectedBlock
    {
        get => _protectedBlock;
        internal set => UpdateChildNotNull(ref _protectedBlock, value);
    }

    /// <summary>
    /// Gets all handlers that are associated to the protected block.
    /// </summary>
    public IList<HandlerClause<TInstruction>> Handlers
    {
        get;
    }

    /// <inheritdoc />
    public override IEnumerable<TreeNodeBase> GetChildren()
    {
        yield return ProtectedBlock;
        foreach (var handler in Handlers)
            yield return handler;
    }

    /// <inheritdoc />
    protected internal override void OnAttach(CompilationUnit<TInstruction> newRoot)
    {
        ProtectedBlock.OnAttach(newRoot);
        for (int i = 0; i < Handlers.Count; i++)
            Handlers[i].OnAttach(newRoot);
    }

    /// <inheritdoc />
    protected internal override void OnDetach(CompilationUnit<TInstruction> oldRoot)
    {
        ProtectedBlock.OnDetach(oldRoot);
        for (int i = 0; i < Handlers.Count; i++)
            Handlers[i].OnDetach(oldRoot);
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