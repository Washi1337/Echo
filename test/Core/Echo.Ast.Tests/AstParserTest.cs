using System.Collections.Generic;
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
    }
}