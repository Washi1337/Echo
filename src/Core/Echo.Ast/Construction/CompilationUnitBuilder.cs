using Echo.ControlFlow;
using Echo.ControlFlow.Blocks;
using Echo.ControlFlow.Serialization.Blocks;

namespace Echo.Ast.Construction;

/// <summary>
/// Provides a mechanism for constructing compilation unit from a lifted control flow graphs.
/// </summary>
/// <typeparam name="TInstruction">The type of instructions stored in the AST.</typeparam>
public class CompilationUnitBuilder<TInstruction> : IBlockVisitor<Statement<TInstruction>, object?, AstNode<TInstruction>>
{
    /// <summary>
    /// Constructs a compilation unit based on a lifted control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to lift.</param>
    /// <returns>The constructed compilation unit.</returns>
    public CompilationUnit<TInstruction> Construct(ControlFlowGraph<Statement<TInstruction>> cfg)
    {
        return Construct(cfg.ConstructBlocks());
    }

    /// <summary>
    /// Constructs a compilation unit based on a lifted block tree.
    /// </summary>
    /// <param name="root">The block tree to lift.</param>
    /// <returns>The constructed compilation unit.</returns>
    public CompilationUnit<TInstruction> Construct(ScopeBlock<Statement<TInstruction>> root) => new()
    {
        Root = (BlockStatement<TInstruction>) root.AcceptVisitor(this, null)
    };

    /// <inheritdoc />
    public AstNode<TInstruction> VisitBasicBlock(BasicBlock<Statement<TInstruction>> block, object? state)
    {
        var result = new BlockStatement<TInstruction>();
        
        foreach (var statement in block.Instructions)
            result.Statements.Add(statement);

        if (result.Statements.Count > 0
            && result.Statements[0].OriginalRange is { } start
            && result.Statements[result.Statements.Count - 1].OriginalRange is { } end)
        {
            result.OriginalRange = new AddressRange(start.Start, end.End);
        }

        return result;
    }

    /// <inheritdoc />
    public AstNode<TInstruction> VisitScopeBlock(ScopeBlock<Statement<TInstruction>> block, object? state)
    {
        var result = new BlockStatement<TInstruction>();

        foreach (var b in block.Blocks)
            result.Statements.Add((Statement<TInstruction>) b.AcceptVisitor(this, state));

        if (result.Statements.Count > 0
            && result.Statements[0].OriginalRange is { } start
            && result.Statements[result.Statements.Count - 1].OriginalRange is { } end)
        {
            result.OriginalRange = new AddressRange(start.Start, end.End);
        }

        return result;
    }

    /// <inheritdoc />
    public AstNode<TInstruction> VisitExceptionHandlerBlock(ExceptionHandlerBlock<Statement<TInstruction>> block, object? state)
    {
        var result = new ExceptionHandlerStatement<TInstruction>();

        result.ProtectedBlock = (BlockStatement<TInstruction>) block.ProtectedBlock.AcceptVisitor(this, state);
        foreach (var handler in block.Handlers)
            result.Handlers.Add((HandlerClause<TInstruction>) handler.AcceptVisitor(this, state));

        if (result.ProtectedBlock.OriginalRange is { } start)
        {
            AddressRange? end = null;
            
            for (int i = result.Handlers.Count - 1; i >= 0; i--)
            {
                if (result.Handlers[i].OriginalRange is { } candidate)
                {
                    end = candidate;
                    break;
                }
            }
            
            end ??= start;
            result.OriginalRange = new AddressRange(start.Start, end.Value.End);
        }

        return result;
    }

    /// <inheritdoc />
    public AstNode<TInstruction> VisitHandlerBlock(HandlerBlock<Statement<TInstruction>> block, object? state)
    {
        var result = new HandlerClause<TInstruction>();

        result.Tag = block.Tag;

        AddressRange? start = null;
        AddressRange? end;

        if (block.Prologue is not null)
        {
            result.Prologue = (BlockStatement<TInstruction>) block.Prologue.AcceptVisitor(this, state);
            start = result.Prologue.OriginalRange;
        }

        result.Contents = (BlockStatement<TInstruction>) block.Contents.AcceptVisitor(this, state);
        start ??= result.Contents.OriginalRange;
        end = result.Contents.OriginalRange;

        if (block.Epilogue is not null)
        {
            result.Epilogue = (BlockStatement<TInstruction>) block.Epilogue.AcceptVisitor(this, state);
            start ??= result.Epilogue.OriginalRange;
            end = result.Epilogue.OriginalRange;
        }

        if (start is not null && end is not null)
            result.OriginalRange = new AddressRange(start.Value.Start, end.Value.End);
        
        return result;
    }
}