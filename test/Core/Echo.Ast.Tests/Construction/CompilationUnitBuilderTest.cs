using Echo.Ast.Construction;
using Echo.ControlFlow.Blocks;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.Ast.Tests.Construction;

public class CompilationUnitBuilderTest
{
    [Fact]
    public void SingleBlock()
    {
        // Prepare
        var root = new ScopeBlock<Statement<DummyInstruction>>
        {
            Blocks =
            {
                new BasicBlock<Statement<DummyInstruction>>
                {
                    Instructions = {Statement.Expression(Expression.Instruction(DummyInstruction.Ret(0)))}
                }
            }
        };

        // Construct
        var unit = root.ToCompilationUnit();
        
        // Verify
        var block = Assert.IsAssignableFrom<BlockStatement<DummyInstruction>>(Assert.Single(unit.Root.Statements));
        Assert.IsAssignableFrom<ExpressionStatement<DummyInstruction>>(Assert.Single(block.Statements));
    }

    [Fact]
    public void TwoBlocks()
    {
        // Prepare
        var root = new ScopeBlock<Statement<DummyInstruction>>
        {
            Blocks =
            {
                new BasicBlock<Statement<DummyInstruction>>
                {
                    Instructions = {Statement.Expression(Expression.Instruction(DummyInstruction.Jmp(0, 1)))}
                },
                new BasicBlock<Statement<DummyInstruction>>
                {
                    Instructions = {Statement.Expression(Expression.Instruction(DummyInstruction.Ret(1)))}
                }
            }
        };

        // Construct
        var unit = root.ToCompilationUnit();
        
        // Verify
        Assert.Equal(2, unit.Root.Statements.Count);
        var block1 = Assert.IsAssignableFrom<BlockStatement<DummyInstruction>>(unit.Root.Statements[0]);
        var block2 = Assert.IsAssignableFrom<BlockStatement<DummyInstruction>>(unit.Root.Statements[1]);
        Assert.IsAssignableFrom<ExpressionStatement<DummyInstruction>>(Assert.Single(block1.Statements));
        Assert.IsAssignableFrom<ExpressionStatement<DummyInstruction>>(Assert.Single(block2.Statements));
    }

    [Fact]
    public void ExceptionHandler()
    {
        // Prepare
        var root = new ScopeBlock<Statement<DummyInstruction>>();
        var eh = new ExceptionHandlerBlock<Statement<DummyInstruction>>();
        eh.ProtectedBlock.Blocks.Add(new BasicBlock<Statement<DummyInstruction>>
        {
            Instructions = {Statement.Expression(Expression.Instruction(DummyInstruction.Jmp(0, 2)))}
        });
        var handler = new HandlerBlock<Statement<DummyInstruction>>();
        handler.Contents.Blocks.Add(new BasicBlock<Statement<DummyInstruction>>
        {
            Instructions = {Statement.Expression(Expression.Instruction(DummyInstruction.Jmp(1, 2)))}
        });
        eh.Handlers.Add(handler);
        root.Blocks.Add(eh);
        root.Blocks.Add(new BasicBlock<Statement<DummyInstruction>>
        {
            Instructions = {Statement.Expression(Expression.Instruction(DummyInstruction.Ret(2)))}
        });

        // Construct
        var unit = root.ToCompilationUnit();
        
        Assert.Equal(2, unit.Root.Statements.Count);
        var ehBlock = Assert.IsAssignableFrom<ExceptionHandlerStatement<DummyInstruction>>(unit.Root.Statements[0]);
        Assert.Single(ehBlock.ProtectedBlock.Statements);
        Assert.Single(ehBlock.Handlers);
        Assert.IsAssignableFrom<BlockStatement<DummyInstruction>>(unit.Root.Statements[1]);
    }
}