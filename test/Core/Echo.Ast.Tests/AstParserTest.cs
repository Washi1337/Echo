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
                StatementPattern.Assignment(
                    Pattern.Any<IVariable>().CaptureAs(variableCapture),
                    ExpressionPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Push))),

                // pop(stack_slot)
                StatementPattern.Expression(ExpressionPattern
                    .Instruction(new DummyInstructionPattern(DummyOpCode.Pop))
                    .WithArguments(ExpressionPattern.Variable<DummyInstruction>().CaptureVariable(variableCapture))),

                // ret()
                StatementPattern.Instruction(new DummyInstructionPattern(DummyOpCode.Ret))
            );

            var result = finalPattern.Match(cfg.Nodes[0].Contents.Instructions);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Captures[variableCapture].Distinct());
        }
    }
}