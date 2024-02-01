using System.Collections.Generic;
using Echo.Graphing;

namespace Echo.Ast;

/// <summary>
/// Represents a single block statement in an AST.
/// </summary>
/// <typeparam name="TInstruction">The type of instruction stored in the AST.</typeparam>
public class BlockStatement<TInstruction> : Statement<TInstruction>
{
    /// <summary>
    /// Creates a new empty block.
    /// </summary>
    public BlockStatement()
    {
        Statements = new TreeNodeCollection<BlockStatement<TInstruction>, Statement<TInstruction>>(this);
    }
    
    /// <summary>
    /// Gets the sequence of statements that are executed in the block. 
    /// </summary>
    public IList<Statement<TInstruction>> Statements
    {
        get;
    }

    /// <inheritdoc />
    public override IEnumerable<TreeNodeBase> GetChildren() => Statements;

    /// <inheritdoc />
    protected internal override void OnAttach(CompilationUnit<TInstruction> newRoot)
    {
        for (int i = 0; i < Statements.Count; i++)
            Statements[i].OnAttach(newRoot);
    }

    /// <inheritdoc />
    protected internal override void OnDetach(CompilationUnit<TInstruction> oldRoot)
    {
        for (int i = 0; i < Statements.Count; i++)
            Statements[i].OnDetach(oldRoot);
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