using System.Collections.Generic;
using Echo.Graphing;

namespace Echo.Ast;

/// <summary>
/// Represents a single compilation unit of code. This is the root node of any AST.
/// </summary>
/// <typeparam name="TInstruction">The type of instructions to store in the AST.</typeparam>
public class CompilationUnit<TInstruction> : AstNode<TInstruction>
{
    private BlockStatement<TInstruction> _root = null!;

    /// <summary>
    /// Creates a new empty compilation unit.
    /// </summary>
    public CompilationUnit()
    {
        Root = new BlockStatement<TInstruction>();
    }
    
    /// <summary>
    /// Gets the root scope of the compilation unit.
    /// </summary>
    public BlockStatement<TInstruction> Root
    {
        get => _root;
        private set => UpdateChildNotNull(ref _root, value);
    }

    /// <inheritdoc />
    public override IEnumerable<TreeNodeBase> GetChildren() => new[] {Root};
    
    /// <inheritdoc />
    public override void Accept(IAstNodeVisitor<TInstruction> visitor) => visitor.Visit(this);

    /// <inheritdoc />
    public override void Accept<TState>(IAstNodeVisitor<TInstruction, TState> visitor, TState state) =>
        visitor.Visit(this, state);

    /// <inheritdoc />
    public override TOut Accept<TState, TOut>(IAstNodeVisitor<TInstruction, TState, TOut> visitor, TState state) =>
        visitor.Visit(this, state);
}