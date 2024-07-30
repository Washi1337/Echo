using System.Collections.Generic;
using System.IO;
using System.Linq;
using Echo.Ast.Construction;
using Echo.Ast.Patterns;
using Echo.Ast.Tests.Patterns;
using Echo.Code;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Regions;
using Echo.ControlFlow.Regions.Detection;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;
using Xunit;

namespace Echo.Ast.Tests.Construction;

public class ControlFlowGraphLifterTest
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
        
        return cfg.Lift(DummyPurityClassifier.Instance);
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

        var arguments = new CaptureGroup<Expression<DummyInstruction>>("arguments");
        
        // pop(push(), push())
        var match = StatementPattern
            .Expression(ExpressionPattern
                .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push)),
                    ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push))
                ).CaptureArguments(arguments)
            )
            .Match(node.Contents.Instructions[0]);
        
        Assert.True(match.IsSuccess);
        Assert.Equal(
            new long[] {0, 1},
            match.GetCaptures(arguments)
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

        var arguments = new CaptureGroup<Expression<DummyInstruction>>("arguments");

        // pop(push())
        var match = StatementPattern
            .Expression(ExpressionPattern
                .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push))
                ).CaptureArguments(arguments)
            )
            .Match(node.Contents.Instructions[0]);
        Assert.True(match.IsSuccess);
        Assert.Equal(
            new long[] {0},
            match.GetCaptures(arguments)
                .Cast<InstructionExpression<DummyInstruction>>()
                .Select(x => x.Instruction.Offset)
        );

        // pop(push(), push())
        match = StatementPattern
            .Expression(ExpressionPattern
                .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push)),
                    ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push))
                ).CaptureArguments(arguments)
            )
            .Match(node.Contents.Instructions[1]);

        Assert.True(match.IsSuccess);
        Assert.Equal(
            new long[] {2, 3},
            match.GetCaptures(arguments)
                .Cast<InstructionExpression<DummyInstruction>>()
                .Select(x => x.Instruction.Offset)
        );
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();

        Assert.True(StatementPattern
            .Assignment(
                new[] {Pattern.Any<IVariable>(), Pattern.Any<IVariable>()},
                ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
            ).Match(offsetMap[0].Contents.Instructions[0]).IsSuccess
        );
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
                .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
                .WithArguments(
                    ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push)),
                    ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
                        .WithArguments(
                            ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push))
                        )
                )
            )
            .Match(node.Contents.Instructions[0]);
        
        Assert.True(match.IsSuccess);
    }
    
    [Fact]
    public void NestedImpureExpressions()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            // op(op(), op())
            DummyInstruction.Op(0, 0, 1),
            DummyInstruction.Op(1, 0, 1),
            DummyInstruction.Op(2, 2, 1),
            
            // op(op(), op())
            DummyInstruction.Op(3, 0, 1),
            DummyInstruction.Op(4, 0, 1),
            DummyInstruction.Op(5, 2, 1),
            
            // op( op(op(), op()) , op(op(), op()) )
            DummyInstruction.Op(6, 2, 0),
            
            // ret()
            DummyInstruction.Ret(7),
        });
        
        // Verify
        var n1 = Assert.Single(cfg.Nodes);
        
        var pattern = StatementPattern.Expression(ExpressionPattern
            .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
            .WithArguments(
                ExpressionPattern
                    .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
                    .WithArguments(
                        ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op)),
                        ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
                    ),
                ExpressionPattern
                    .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
                    .WithArguments(
                        ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op)),
                        ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
                    )
            )
        );
        
        Assert.True(pattern.Match(n1.Contents.Instructions[0]).IsSuccess);
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
        var variable = new CaptureGroup<IVariable>("variable");

        // tmp1 = push()
        // tmp2 = push()
        var pattern1 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variable),
                ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push))
            );
        var match1 = pattern1.Match(node.Contents.Instructions[0]);
        var match2 = pattern1.Match(node.Contents.Instructions[1]);

        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);

        // op(push())
        Assert.True(StatementPattern
            .Expression(ExpressionPattern
                .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Op))
                .WithArguments(1)
            )
            .Match(node.Contents.Instructions[2]).IsSuccess);

        // pop(tmp)
        var pattern2 = StatementPattern
            .Expression(ExpressionPattern
                .Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(ExpressionPattern.Variable<DummyInstruction>(
                    Pattern.Any<IVariable>().CaptureAs(variable))
                )
            );
        var match3 = pattern2.Match(node.Contents.Instructions[3]);
        var match4 = pattern2.Match(node.Contents.Instructions[4]);
        
        Assert.True(match3.IsSuccess);
        Assert.True(match4.IsSuccess);
        
        Assert.Same(match1.GetCaptures(variable)[0], match4.GetCaptures(variable)[0]);
        Assert.Same(match2.GetCaptures(variable)[0], match3.GetCaptures(variable)[0]);
    }
    
    [Fact]
    public void Test()
    {
        // Construct
        var cfg = ConstructGraph(new[]
        {
            DummyInstruction.Push(0, 1),
            DummyInstruction.Op(1, 0, 1),
            DummyInstruction.Op(2, 0, 1),
            DummyInstruction.Op(3, 1, 1),
            DummyInstruction.Op(4, 0, 1),
            DummyInstruction.Op(5, 4, 0),
            
            // ret()
            DummyInstruction.Ret(6)
        });
        
        using var fs = File.CreateText("/tmp/output.dot");
        cfg.ToDotGraph(fs, new DummyFormatter { IncludeOffset = false }.ToAstFormatter());
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        Assert.Equal(2, cfg.Nodes.Count);
        var (n1, n2) = (offsetMap[0], offsetMap[10]);

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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        Assert.Equal(2, cfg.Nodes.Count);
        var (n1, n2) = (offsetMap[0], offsetMap[10]);

        var variable = new CaptureGroup<IVariable>("variable");
        
        // out = push()
        var match1 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variable),
                 ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push))
            )
            .Match(n1.Contents.Instructions[0]);
        Assert.True(match1.IsSuccess);
        
        // pop(out)
        var match2 = StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Variable<DummyInstruction>(Pattern.Any<IVariable>().CaptureAs(variable))
                )
            )
            .Match(n2.Contents.Instructions[0]);
        Assert.True(match2.IsSuccess);
        
        Assert.Same(match1.GetCaptures(variable)[0], match2.GetCaptures(variable)[0]);
        Assert.Same(match2.GetCaptures(variable)[0], match2.GetCaptures(variable)[0]);

        Assert.Same(n2, n1.UnconditionalNeighbour);
    }

    [Fact]
    public void TwoNodesWithIndirectStackDeltaShouldInline()
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();

        var variable = new CaptureGroup<IVariable>();

        var match1 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variable),
                ExpressionPattern.Any<DummyInstruction>()
            )
            .FollowedBy(ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Op)).ToStatement())
            .FollowedBy(ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Jmp)).ToStatement())
            .Match(offsetMap[0].Contents.Instructions);

        var match2 = StatementPattern
            .Expression(ExpressionPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                .WithArguments(
                    ExpressionPattern.Variable<DummyInstruction>(Pattern.Any<IVariable>().CaptureAs(variable))
                )
            )
            .Match(offsetMap[10].Contents.Instructions[0]);

        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();

        var block = offsetMap[0].Contents;
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        // Verify
        var variable = new CaptureGroup<IVariable>("variable");
        var value = new CaptureGroup<Expression<DummyInstruction>>("value");
        var pattern = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variable),
            ExpressionPattern.Instruction<DummyInstruction>(new DummyInstructionPattern(DummyOpCode.Push)).CaptureAs(value)
        );

        // Ensure expressions are pushed as variables.
        var match1 = pattern.Match(offsetMap[0].Contents.Instructions[0]);
        var match2 = pattern.Match(offsetMap[0].Contents.Instructions[1]);
        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);

        // Ensure order of operations is preserved.
        var a = (InstructionExpression<DummyInstruction>) match1.GetCaptures(value)[0];
        var b = (InstructionExpression<DummyInstruction>) match2.GetCaptures(value)[0];
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();

        // Verify graph structure.
        var (n1, n2, n3, n4) = (offsetMap[0], offsetMap[2], offsetMap[10], offsetMap[11]);
        Assert.Same(n2, n1.UnconditionalNeighbour);
        Assert.Same(n3, Assert.Single(n1.ConditionalEdges).Target);
        Assert.Same(n4, n2.UnconditionalNeighbour);
        Assert.Same(n4, n3.UnconditionalNeighbour);

        // Verify phi variables.
        var variableCapture = new CaptureGroup<IVariable>("variables");
        var sourcesCapture = new CaptureGroup<VariableExpression<DummyInstruction>>("sources");

        var match1 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variableCapture),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n2.Contents.Instructions[0]);
        var match2 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variableCapture),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n3.Contents.Instructions[0]);
        var match3 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(2)
            .CaptureSources(sourcesCapture)
            .Match(n4.Contents.Instructions[0]);

        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
        Assert.True(match3.IsSuccess);
        
        var sources = match3.GetCaptures(sourcesCapture)
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.GetCaptures(variableCapture)[0], sources);
        Assert.Contains(match2.GetCaptures(variableCapture)[0], sources);
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        // Verify graph structure.
        var (n1, n2, n3, n4, n5, n6) = 
            (offsetMap[0], offsetMap[2], offsetMap[4], offsetMap[10], offsetMap[20], offsetMap[21]);
        
        Assert.Same(n2, n1.UnconditionalNeighbour);
        Assert.Same(n5, Assert.Single(n1.ConditionalEdges).Target);
        
        Assert.Same(n3, n2.UnconditionalNeighbour);
        Assert.Same(n4, Assert.Single(n2.ConditionalEdges).Target);
        
        Assert.Same(n6, n3.UnconditionalNeighbour);
        Assert.Same(n6, n4.UnconditionalNeighbour);
        Assert.Same(n6, n5.UnconditionalNeighbour);
        
        // Verify phi variables.
        var variablesCapture = new CaptureGroup<IVariable>("variables");
        var sourcesCapture = new CaptureGroup<VariableExpression<DummyInstruction>>("sources");

        var match1 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variablesCapture),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n3.Contents.Instructions[0]);
        var match2 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variablesCapture),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n4.Contents.Instructions[0]);
        var match3 = StatementPattern.Assignment(
            Pattern.Any<IVariable>().CaptureAs(variablesCapture),
            ExpressionPattern.Any<DummyInstruction>()
        ).Match(n5.Contents.Instructions[0]);
        var match4 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(3)
            .CaptureSources(sourcesCapture)
            .Match(n6.Contents.Instructions[0]);

        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
        Assert.True(match3.IsSuccess);
        Assert.True(match4.IsSuccess);
        
        var sources = match4.GetCaptures(sourcesCapture)
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.GetCaptures(variablesCapture)[0], sources);
        Assert.Contains(match2.GetCaptures(variablesCapture)[0], sources);
        Assert.Contains(match3.GetCaptures(variablesCapture)[0], sources);
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        var variablesCapture = new CaptureGroup<IVariable>("variables");
        var sourcesCapture = new CaptureGroup<VariableExpression<DummyInstruction>>("sources");

        var match1 = StatementPattern
            .Assignment<DummyInstruction>()
            .WithVariables(
                Pattern.Any<IVariable>().CaptureAs(variablesCapture),
                Pattern.Any<IVariable>()
            )
            .Match(offsetMap[0].Contents.Instructions[0]);

        var match2 = StatementPattern
            .Assignment<DummyInstruction>()
            .WithVariables(1)
            .CaptureVariables(variablesCapture)
            .Match(offsetMap[3].Contents.Instructions[^1]);
        
        var match3 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(2)
            .CaptureSources(sourcesCapture)
            .Match(offsetMap[5].Contents.Instructions[0]);
        
        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
        Assert.True(match3.IsSuccess);

        var sources = match3.GetCaptures(sourcesCapture)
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.GetCaptures(variablesCapture)[0], sources);
        Assert.Contains(match2.GetCaptures(variablesCapture)[0], sources);
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        var variablesCapture = new CaptureGroup<IVariable>("variables");
        var sourcesCapture = new CaptureGroup<VariableExpression<DummyInstruction>>("sources");
        
        var match1 = StatementPattern
            .Assignment<DummyInstruction>()
            .WithVariables(
                Pattern.Any<IVariable>().CaptureAs(variablesCapture),
                Pattern.Any<IVariable>()
            )
            .Match(offsetMap[0].Contents.Instructions[^2]);
     
        var match2 = StatementPattern
                .Assignment<DummyInstruction>()
                .WithVariables(1)
                .CaptureVariables(variablesCapture)
                .Match(offsetMap[4].Contents.Instructions[^1]);

        var match3 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(2)
            .CaptureSources(sourcesCapture)
            .Match(offsetMap[6].Contents.Instructions[0]);
        
        Assert.True(match1.IsSuccess);
        Assert.True(match3.IsSuccess);

        var sources = match3.GetCaptures(sourcesCapture)
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.GetCaptures(variablesCapture)[0], sources);
        Assert.Contains(match2.GetCaptures(variablesCapture)[0], sources);
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();

        // Verify
        var (n1, n2, n3) = (offsetMap[0], offsetMap[1], offsetMap[4]);
        
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        // Verify
        var (n1, n2, n3) = (offsetMap[0], offsetMap[1], offsetMap[5]);
        Assert.Same(n2, n1.UnconditionalNeighbour);
        Assert.Same(n2, Assert.Single(n2.ConditionalEdges).Target);
        Assert.Same(n3, n2.UnconditionalNeighbour);

        var variableCapture = new CaptureGroup<IVariable>("variable");
        var sourcesCapture = new CaptureGroup<VariableExpression<DummyInstruction>>("source");

        var match1 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variableCapture),
                ExpressionPattern.Any<DummyInstruction>())
            .Match(n1.Contents.Instructions[0]);
        var match2 = StatementPattern.Phi<DummyInstruction>()
            .WithSources(2)
            .CaptureSources(sourcesCapture)
            .Match(n2.Contents.Instructions[0]);
        var match3 = StatementPattern
            .Assignment(
                Pattern.Any<IVariable>().CaptureAs(variableCapture),
                ExpressionPattern.Any<DummyInstruction>())
            .Match(n2.Contents.Instructions[^2]);
        
        Assert.True(match1.IsSuccess);
        Assert.True(match2.IsSuccess);
        Assert.True(match3.IsSuccess);
        
        var sources = match2.GetCaptures(sourcesCapture)
            .Select(x => x.Variable)
            .ToArray();
        
        Assert.Contains(match1.GetCaptures(variableCapture)[0], sources);
        Assert.Contains(match3.GetCaptures(variableCapture)[0], sources);
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        // Verify
        var (n1, n2, n3, n4) = (offsetMap[0], offsetMap[1], offsetMap[3], offsetMap[5]);
        
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
        var offsetMap = cfg.Nodes.CreateOffsetMap();
        
        // Verify
        var (n1, n2, n3, n4) = (offsetMap[0], offsetMap[1], offsetMap[3], offsetMap[6]);
        
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