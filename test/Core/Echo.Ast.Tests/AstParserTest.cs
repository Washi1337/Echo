using System.Collections.Generic;
using System.Linq;
using Echo.Ast.Patterns;
using Echo.Ast.Tests.Patterns;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Core.Code;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;
using Xunit;

namespace Echo.Ast.Tests
{
    public class AstParserTest
    {
        private ControlFlowGraph<StatementBase<DummyInstruction>> ConstructAst(
            IEnumerable<DummyInstruction> instructions)
        {
            var architecture = DummyArchitecture.Instance;
            
            var dfgBuilder = new DummyTransitionResolver();
            var cfgBuilder = new SymbolicFlowGraphBuilder<DummyInstruction>(architecture, instructions, dfgBuilder);

            var cfg = cfgBuilder.ConstructFlowGraph(0);
            var astBuilder = new AstParser<DummyInstruction>(cfg, dfgBuilder.DataFlowGraph);
            return astBuilder.Parse();
        }
        
        [Fact]
        public void SingleInstructionNoArgumentsShouldResultInSingleExpressionStatement()
        {
            var cfg = ConstructAst(new[]
            {
                DummyInstruction.Ret(0)
            });

            var pattern = StatementPattern
                .Instruction(new DummyInstructionPattern(DummyOpCode.Ret));

            var result = pattern.Match(cfg.Nodes[0].Contents.Header);
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void InstructionWithOneStackArgumentShouldResultInAssignmentAndExpressionStatementWithArgument()
        {
            var cfg = ConstructAst(new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Pop(1, 1), 
                DummyInstruction.Ret(2)
            });

            var variableCapture = new CaptureGroup("variable");

            var finalPattern = new SequencePattern<StatementBase<DummyInstruction>>(
                // stack_slot = push 1()
                StatementPattern
                    .Assignment<DummyInstruction>()
                    .WithExpression(ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push)))
                    .CaptureVariables(variableCapture),

                // pop(stack_slot)
                StatementPattern.Expression(
                    ExpressionPattern
                        .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                        .WithArguments(ExpressionPattern
                            .Variable<DummyInstruction>()
                            .CaptureVariable(variableCapture))),

                // ret()
                StatementPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Ret))
            );

            var result = finalPattern.Match(cfg.Nodes[0].Contents.Instructions);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Captures[variableCapture].Distinct());
        }

        [Fact]
        public void JoiningControlFlowPathsWithPositiveStackDeltaShouldResultInPhiNode()
        {
            var cfg = ConstructAst(new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.JmpCond(1,4),

                DummyInstruction.Push(2, 1),
                DummyInstruction.Jmp(3, 5),

                DummyInstruction.Push(4, 1),

                DummyInstruction.Pop(5, 1),
                DummyInstruction.Ret(6)
            });
            
            var phiSourcesCapture = new CaptureGroup("sources");
            var variablesCapture = new CaptureGroup("variables");

            // stack_slot = push 1()
            var assignPattern = StatementPattern
                .Assignment<DummyInstruction>()
                .CaptureVariables(variablesCapture);
            
            // variable = phi(stack_slot_1,stack_slot_2) 
            var phiPattern = StatementPattern
                .Phi<DummyInstruction>()
                .WithSources(2)
                .CaptureSources(phiSourcesCapture);

            var push1Result = assignPattern.Match(cfg.Nodes[2].Contents.Header);
            var push2Result = assignPattern.Match(cfg.Nodes[4].Contents.Header);
            var phiResult = phiPattern.Match(cfg.Nodes[5].Contents.Header);
            
            Assert.True(push1Result.IsSuccess, "Node 2 was expected to start with an assignment statement.");
            Assert.True(push2Result.IsSuccess, "Node 4 was expected to start with an assignment statement.");
            Assert.True(phiResult.IsSuccess, "Node 5 was expected to start with a phi statement.");

            var sources = phiResult.Captures[phiSourcesCapture]
                .Cast<VariableExpression<DummyInstruction>>()
                .Select(s => s.Variable);

            var allVariables = new[]
            {
                (IVariable) push1Result.Captures[variablesCapture][0],
                (IVariable) push2Result.Captures[variablesCapture][0]
            };

            Assert.Equal(allVariables.ToHashSet(), sources.ToHashSet());
        }
    }
}