using System.Collections.Generic;
using System.IO;
using System.Linq;
using Echo.Ast.Construction;
using Echo.Ast.Patterns;
using Echo.Ast.Tests.Patterns;
using Echo.Code;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Static;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Regions.Detection;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;
using Xunit;

namespace Echo.Ast.Tests.Construction;

public class AstBuilderTest
{
    private static ControlFlowGraph<Statement<DummyInstruction>> ConstructGraph(
        IList<DummyInstruction> instructions,
        params ExceptionHandlerRange[] exceptionHandlerRanges)
    {
        var cfg = new StaticFlowGraphBuilder<DummyInstruction>(
                DummyArchitecture.Instance,
                instructions,
                DummyStaticSuccessorResolver.Instance)
            .ConstructFlowGraph(0, exceptionHandlerRanges);

        cfg.DetectExceptionHandlerRegions(exceptionHandlerRanges);
        
        return cfg.ToAst(DummyPurityClassifier.Instance);
    }
    
    [Fact]
    public void StatementNoArguments()
    {
        var result = ConstructGraph(new[]
        {
            DummyInstruction.Ret(0), 
        });

        var node = Assert.Single(result.Nodes);
        var statement = Assert.Single(node.Contents.Instructions);

        var pattern = StatementPattern
            .Instruction(new DummyInstructionPattern(DummyOpCode.Ret));
        Assert.True(pattern.Match(statement).IsSuccess);
    }
    
    [Fact]
    public void StatementWithTwoArguments()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            // pop(push(), push())
            DummyInstruction.Push(0, 1),
            DummyInstruction.Push(1, 1),
            DummyInstruction.Pop(2, 2),
            
            // ret()
            DummyInstruction.Ret(3)
        });

        // Verify
        var node = Assert.Single(cfg.Nodes);

        var arguments = new CaptureGroup("arguments");
        
        // pop(push(), push())
        var match = StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push)),
                    ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push))
                ).CaptureArguments(arguments)
            )
            .Match(node.Contents.Instructions[0]);
        
        Assert.True(match.IsSuccess);
        Assert.Equal(
            new long[] {0, 1},
            match.Captures[arguments]
                .Cast<InstructionExpression<DummyInstruction>>()
                .Select(x => x.Instruction.Offset));
    }

    [Fact]
    public void TwoStatementWithDifferentArguments()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            // pop(push())
            DummyInstruction.Push(0, 1),
            DummyInstruction.Pop(1, 1),

            // pop(push(), push())
            DummyInstruction.Push(2, 1),
            DummyInstruction.Push(3, 1),
            DummyInstruction.Pop(4, 2),

            // ret()
            DummyInstruction.Ret(5)
        });

        // Verify
        var node = Assert.Single(cfg.Nodes);

        var arguments = new CaptureGroup("arguments");

        // pop(push())
        var match = StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push))
                ).CaptureArguments(arguments)
            )
            .Match(node.Contents.Instructions[0]);
        Assert.True(match.IsSuccess);
        Assert.Equal(
            new long[] {0},
            match.Captures[arguments]
                .Cast<InstructionExpression<DummyInstruction>>()
                .Select(x => x.Instruction.Offset));

        // pop(push(), push())
        match = StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push)),
                    ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push))
                ).CaptureArguments(arguments)
            )
            .Match(node.Contents.Instructions[1]);

        Assert.True(match.IsSuccess);
        Assert.Equal(
            new long[] {2, 3},
            match.Captures[arguments]
                .Cast<InstructionExpression<DummyInstruction>>()
                .Select(x => x.Instruction.Offset));
    }

    [Fact]
    public void ExpressionWithMultipleReturnValues()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            // x, y = op()
            DummyInstruction.Op(0, 0, 2),
            
            // pop(y)
            // pop(x)
            DummyInstruction.Pop(1, 1),
            DummyInstruction.Pop(2, 1),

            // ret()
            DummyInstruction.Ret(3)
        });

        Assert.True(StatementPattern
            .Assignment(
                new[] {Pattern.Any<IVariable>(), Pattern.Any<IVariable>()},
                ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Op))
            ).Match(cfg.Nodes[0].Contents.Instructions[0]).IsSuccess);
    }
    
    [Fact]
    public void NestedExpressions()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            // op(push(), op(push()))
            DummyInstruction.Push(0, 1),
            DummyInstruction.Push(1, 1),
            DummyInstruction.Op(2, 1, 1),
            DummyInstruction.Op(3, 2, 0),

            // ret()
            DummyInstruction.Ret(4)
        });
        
        // Verify
        var node = Assert.Single(cfg.Nodes);

        var match = StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Op))
                .WithArguments(
                    ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push)),
                    ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Op))
                        .WithArguments(
                            ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push))
                        )
                )
            )
            .Match(node.Contents.Instructions[0]);
        
        Assert.True(match.IsSuccess);
    }
    
    [Fact]
    public void PushArgumentBeforeImpureStatement()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            // tmp1 = push()
            // tmp2 = push()
            DummyInstruction.Push(0, 1),
            DummyInstruction.Push(1, 1),

            // op(push())
            DummyInstruction.Push(2, 1),
            DummyInstruction.Op(3, 1, 0),
            
            // pop(tmp2)
            // pop(tmp1)
            DummyInstruction.Pop(4, 1),
            DummyInstruction.Pop(5, 1),

            // ret()
            DummyInstruction.Ret(6)
        });
        
        // Verify
        var node = Assert.Single(cfg.Nodes);
        var variable = new CaptureGroup("variable");

        // tmp1 = push()
        // tmp2 = push()
        var pattern1 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variable),
                ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push))
            );
        var match1 = pattern1.Match(node.Contents.Instructions[0]);
        var match2 = pattern1.Match(node.Contents.Instructions[1]);

        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);

        // op(push())
        Assert.True(StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Op))
                .WithArguments(1)
            )
            .Match(node.Contents.Instructions[2]).IsSuccess);

        // pop(tmp)
        var pattern2 = StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(ExpressionPattern.Variable<DummyInstruction>(
                    Pattern.Any<IVariable>().CaptureAs(variable))
                )
            );
        var match3 = pattern2.Match(node.Contents.Instructions[3]);
        var match4 = pattern2.Match(node.Contents.Instructions[4]);
        
        Assert.True(match3.IsSuccess);
        Assert.True(match4.IsSuccess);
        
        Assert.Same(match1.Captures[variable][0], match4.Captures[variable][0]);
        Assert.Same(match2.Captures[variable][0], match3.Captures[variable][0]);
    }

    [Fact]
    public void TwoNodes()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Op(0, 0,0),
            DummyInstruction.Jmp(1, 10), 

            DummyInstruction.Op(10, 0,0),
            DummyInstruction.Ret(11)
        });
        
        Assert.Equal(2, cfg.Nodes.Count);
        var (n1, n2) = (cfg.Nodes[0], cfg.Nodes[10]);

        Assert.True(StatementPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Op))
            .Match(n1.Contents.Instructions[0])
            .IsSuccess);
        Assert.True(StatementPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Jmp))
            .Match(n1.Contents.Instructions[1])
            .IsSuccess);
        
        Assert.True(StatementPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Op))
            .Match(n2.Contents.Instructions[0])
            .IsSuccess);
        Assert.True(StatementPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Ret))
            .Match(n2.Contents.Instructions[1])
            .IsSuccess);
        
        Assert.Same(n2, n1.UnconditionalNeighbour);
    }

    [Fact]
    public void TwoNodesWithStackDeltas()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Push(0, 1),
            DummyInstruction.Jmp(1, 10), 

            DummyInstruction.Pop(10, 1),
            DummyInstruction.Ret(11)
        });
        
        Assert.Equal(2, cfg.Nodes.Count);
        var (n1, n2) = (cfg.Nodes[0], cfg.Nodes[10]);

        var variable = new CaptureGroup("variable");
        
        // out = push()
        var match1 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variable),
                 ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push))
            )
            .Match(n1.Contents.Instructions[0]);
        Assert.True(match1.IsSuccess);

        // in = out
        var match2 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variable),
                 ExpressionPattern.Variable<DummyInstruction>(Pattern.Any<IVariable>().CaptureAs(variable))
            )
            .Match(n2.Contents.Instructions[0]);
        Assert.True(match2.IsSuccess);
        
        // pop(in)
        var match3 = StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Variable<DummyInstruction>(Pattern.Any<IVariable>().CaptureAs(variable))
                )
            )
            .Match(n2.Contents.Instructions[1]);
        Assert.True(match3.IsSuccess);
        
        Assert.Same(match1.Captures[variable][0], match2.Captures[variable][1]);
        Assert.Same(match2.Captures[variable][0], match3.Captures[variable][0]);

        Assert.Same(n2, n1.UnconditionalNeighbour);
    }

    [Fact]
    public void TwoNodesPushBeforeImpure()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Push(0, 1),
            DummyInstruction.Op(1, 0, 0),
            DummyInstruction.Jmp(2, 10), 

            DummyInstruction.Pop(10, 1),
            DummyInstruction.Ret(11)
        });

        using var fs = File.CreateText("/tmp/output.dot");
        cfg.ToDotGraph(fs);

        var block = cfg.Nodes[0].Contents;
        Assert.IsAssignableFrom<AssignmentStatement<DummyInstruction>>(block.Instructions[0]);
        Assert.IsAssignableFrom<ExpressionStatement<DummyInstruction>>(block.Instructions[1]);
    }

    [Fact]
    public void TwoNodesWithNestedStackDelta()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Push(0, 1),
            DummyInstruction.Push(1, 1),
            DummyInstruction.Jmp(2, 10), 

            DummyInstruction.Pop(10, 1),
            DummyInstruction.Pop(11, 1),
            DummyInstruction.Ret(12)
        });
        
        // Verify
        var variable = new CaptureGroup("variable");
        var value = new CaptureGroup("value");
        var pattern = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variable),
            ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push)).CaptureAs(value)
        );

        // Ensure expressions are pushed as variables.
        var match1 = pattern.Match(cfg.Nodes[0].Contents.Instructions[0]);
        var match2 = pattern.Match(cfg.Nodes[0].Contents.Instructions[1]);
        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);

        // Ensure order of operations is preserved.
        var a = (InstructionExpression<DummyInstruction>) match1.Captures[value][0];
        var b = (InstructionExpression<DummyInstruction>) match2.Captures[value][0];
        Assert.Equal(0, a.Instruction.Offset);
        Assert.Equal(1, b.Instruction.Offset);
    }

    [Fact]
    public void StackDeltaConvergingControlFlowPaths()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            // jmpcond(push())
            DummyInstruction.Push(0, 1),
            DummyInstruction.JmpCond(1, 10), 

            // x = push()
            DummyInstruction.Push(2, 1),
            DummyInstruction.Jmp(3, 11), 
            
            // y = push()
            DummyInstruction.Push(10, 1),
            
            // pop(phi(x, y))
            DummyInstruction.Pop(11, 1),
            DummyInstruction.Ret(12)
        });

        // Verify graph structure.
        var (n1, n2, n3, n4) = (cfg.Nodes[0], cfg.Nodes[2], cfg.Nodes[10], cfg.Nodes[11]);
        Assert.Same(n2, n1.UnconditionalNeighbour);
        Assert.Same(n3, Assert.Single(n1.ConditionalEdges).Target);
        Assert.Same(n4, n2.UnconditionalNeighbour);
        Assert.Same(n4, n3.UnconditionalNeighbour);

        // Verify phi variables.
        var variable = new CaptureGroup("variables");

        var match1 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variable),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n2.Contents.Instructions[0]);
        var match2 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variable),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n3.Contents.Instructions[0]);
        var match3 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(2)
            .CaptureSources(variable)
            .Match(n4.Contents.Instructions[0]);

        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
        Assert.True(match3.IsSuccess);
        
        var sources = match3.Captures[variable]
            .OfType<VariableExpression<DummyInstruction>>()
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.Captures[variable][0], sources);
        Assert.Contains(match2.Captures[variable][0], sources);
    }

    [Fact]
    public void StackDeltaTwoLayers()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            // jmpcond(push())
            DummyInstruction.Push(0, 1),
            DummyInstruction.JmpCond(1, 20), 
            
            // jmpcond(push())
            DummyInstruction.Push(2, 1),
            DummyInstruction.JmpCond(3, 10), 

            // x = push()
            DummyInstruction.Push(4, 1),
            DummyInstruction.Jmp(5, 21), 
            
            // y = push()
            DummyInstruction.Push(10, 1),
            DummyInstruction.Jmp(11, 21),
            
            // z = push()
            DummyInstruction.Push(20, 1),
            
            // pop(phi(x, y, z))
            DummyInstruction.Pop(21, 1),
            DummyInstruction.Ret(22)
        });
        
        // Verify graph structure.
        var (n1, n2, n3, n4, n5, n6) = 
            (cfg.Nodes[0], cfg.Nodes[2], cfg.Nodes[4], cfg.Nodes[10], cfg.Nodes[20], cfg.Nodes[21]);
        
        Assert.Same(n2, n1.UnconditionalNeighbour);
        Assert.Same(n5, Assert.Single(n1.ConditionalEdges).Target);
        
        Assert.Same(n3, n2.UnconditionalNeighbour);
        Assert.Same(n4, Assert.Single(n2.ConditionalEdges).Target);
        
        Assert.Same(n6, n3.UnconditionalNeighbour);
        Assert.Same(n6, n4.UnconditionalNeighbour);
        Assert.Same(n6, n5.UnconditionalNeighbour);
        
        // Verify phi variables.
        var variable = new CaptureGroup("variables");

        var match1 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variable),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n3.Contents.Instructions[0]);
        var match2 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variable),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n4.Contents.Instructions[0]);
        var match3 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variable),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n5.Contents.Instructions[0]);
        var match4 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(3)
            .CaptureSources(variable)
            .Match(n6.Contents.Instructions[0]);

        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
        Assert.True(match3.IsSuccess);
        Assert.True(match4.IsSuccess);
        
        var sources = match4.Captures[variable]
            .OfType<VariableExpression<DummyInstruction>>()
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.Captures[variable][0], sources);
        Assert.Contains(match2.Captures[variable][0], sources);
        Assert.Contains(match3.Captures[variable][0], sources);
    }

    [Fact]
    public void ConditionalReplaceStackSlot()
    {
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Push(0, 1),
            DummyInstruction.Op(1, 1, 2),
            DummyInstruction.JmpCond(2, 5),

            DummyInstruction.Pop(3, 1),
            DummyInstruction.Push(4, 1),

            DummyInstruction.Pop(5, 1),
            DummyInstruction.Ret(6),
        });
        
        var variables = new CaptureGroup("variables");

        var pattern = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variables),
            ExpressionPattern.Any<DummyInstruction>()
        );
        
        var match1 = pattern.Match(cfg.Nodes[0].Contents.Instructions[1]);
        var match2 = pattern.Match(cfg.Nodes[3].Contents.Instructions[^1]);
        
        var match3 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(2)
            .CaptureSources(variables)
            .Match(cfg.Nodes[5].Contents.Instructions[0]);
        
        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
        Assert.True(match3.IsSuccess);

        var sources = match3.Captures[variables]
            .OfType<VariableExpression<DummyInstruction>>()
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.Captures[variables][0], sources);
        Assert.Contains(match2.Captures[variables][0], sources);
    }

    [Fact]
    public void ConditionalReplaceStackSlotNested()
    {
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Push(0, 1), 
            
            DummyInstruction.Push(1, 1),
            DummyInstruction.Op(2, 1, 2),
            DummyInstruction.JmpCond(3, 6),

            DummyInstruction.Pop(4, 1),
            DummyInstruction.Push(5, 1),

            DummyInstruction.Pop(6, 2),
            DummyInstruction.Ret(7),
        });
        
        var variables = new CaptureGroup("variables");

        var pattern = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variables),
            ExpressionPattern.Any<DummyInstruction>()
        );
        
        var match1 = pattern.Match(cfg.Nodes[0].Contents.Instructions[^2]);
        var match2 = pattern.Match(cfg.Nodes[4].Contents.Instructions[^1]);

        var match3 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(2)
            .CaptureSources(variables)
            .Match(cfg.Nodes[6].Contents.Instructions[1]);
        
        Assert.True(match1.IsSuccess);
        Assert.True(match3.IsSuccess);

        var sources = match3.Captures[variables]
            .OfType<VariableExpression<DummyInstruction>>()
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.Captures[variables][0], sources);
        Assert.Contains(match2.Captures[variables][0], sources);
    }
    
    [Fact]
    public void Loop()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Op(0, 0, 0),
            
            DummyInstruction.Op(1, 0, 0),
            
            DummyInstruction.Push(2, 1),
            DummyInstruction.JmpCond(3, 1),
            
            DummyInstruction.Ret(4), 
        });

        // Verify
        var (n1, n2, n3) = (cfg.Nodes[0], cfg.Nodes[1], cfg.Nodes[4]);
        
        Assert.Same(n2, n1.UnconditionalNeighbour);
        Assert.Same(n2, Assert.Single(n2.ConditionalEdges).Target);
        Assert.Same(n3, n2.UnconditionalNeighbour);
    }

    [Fact]
    public void LoopWithStackDelta()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Push(0, 1),
            
            // loop start:
            DummyInstruction.Op(1, 1, 0),
            
            DummyInstruction.Push(2, 1),
            
            DummyInstruction.Push(3, 1),
            DummyInstruction.JmpCond(4, 1),
            
            DummyInstruction.Ret(5), 
        });
        
        // Verify
        var (n1, n2, n3) = (cfg.Nodes[0], cfg.Nodes[1], cfg.Nodes[5]);
        Assert.Same(n2, n1.UnconditionalNeighbour);
        Assert.Same(n2, Assert.Single(n2.ConditionalEdges).Target);
        Assert.Same(n3, n2.UnconditionalNeighbour);

        var variable = new CaptureGroup("variable");

        var match1 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variable),
                ExpressionPattern.Any<DummyInstruction>())
            .Match(n1.Contents.Instructions[0]);
        var match2 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(2)
            .CaptureSources(variable)
            .Match(n2.Contents.Instructions[0]);
        var match3 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variable),
                ExpressionPattern.Any<DummyInstruction>())
            .Match(n2.Contents.Instructions[^2]);
        
        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
        Assert.True(match3.IsSuccess);
        
        var sources = match2.Captures[variable]
            .OfType<VariableExpression<DummyInstruction>>()
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.Captures[variable][0], sources);
        Assert.Contains(match3.Captures[variable][0], sources);
    }

    [Fact]
    public void Handler()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Op(0, 0, 0),

            // try start:
            DummyInstruction.Op(1, 0, 0),
            DummyInstruction.Jmp(2, 5),

            // handler start
            DummyInstruction.Op(3, 0, 0),
            DummyInstruction.Jmp(4, 5),

            DummyInstruction.Ret(5),
        }, new ExceptionHandlerRange(
            new AddressRange(1, 3),
            new AddressRange(3, 5)
        ));
        
        // Verify
        var (n1, n2, n3, n4) = (cfg.Nodes[0], cfg.Nodes[1], cfg.Nodes[3], cfg.Nodes[5]);
        
        var eh = Assert.IsAssignableFrom<ExceptionHandlerRegion<Statement<DummyInstruction>>>(Assert.Single(cfg.Regions));
        Assert.Same(n2, eh.ProtectedRegion.EntryPoint);
        Assert.Same(n3, Assert.Single(eh.Handlers).GetEntryPoint());
    }

    [Fact]
    public void HandlerPopException()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Op(0, 0, 0),

            // try start
            DummyInstruction.Op(1, 0, 0),
            DummyInstruction.Jmp(2, 6),

            // handler start
            DummyInstruction.Pop(3, 1),
            DummyInstruction.Op(4, 0, 0),
            DummyInstruction.Jmp(5, 6),

            DummyInstruction.Ret(6),
        }, new ExceptionHandlerRange(
            new AddressRange(1, 3),
            new AddressRange(3, 6)
        ));
        
        // Verify
        var (n1, n2, n3, n4) = (cfg.Nodes[0], cfg.Nodes[1], cfg.Nodes[3], cfg.Nodes[6]);
        
        var eh = Assert.IsAssignableFrom<ExceptionHandlerRegion<Statement<DummyInstruction>>>(Assert.Single(cfg.Regions));
        Assert.Same(n2, eh.ProtectedRegion.EntryPoint);
        Assert.Same(n3, Assert.Single(eh.Handlers).GetEntryPoint());
    }

    [Fact]
    public void StackUnderflow()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Pop(0,1),
            DummyInstruction.Ret(1),
        });

        var n1 = Assert.Single(cfg.Nodes);
        Assert.True(StatementPattern
            .Phi<DummyInstruction>()
            .WithSources(0)
            .Match(n1.Contents.Instructions[0])
            .IsSuccess);
    }

}