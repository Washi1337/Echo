using System;
using System.Collections.Generic;
using Echo.ControlFlow;
using Echo.ControlFlow.Construction;
using Echo.ControlFlow.Construction.Symbolic;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.ControlFlow;
using Xunit;

namespace Echo.Ast.Tests
{
    public class AstModelTest
    {
        private ControlFlowGraph<Statement<DummyInstruction>> ConstructAst(
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
        public void RootNodesShouldHaveNoParent()
        {
            var ast = ConstructAst(new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Push(1, 2),
                DummyInstruction.Op(2, 2, 1),
                DummyInstruction.Op(3, 2, 0),
                DummyInstruction.Ret(4)
            });

            Assert.Single(ast.Nodes);
            var block = ast.Entrypoint.Contents.Instructions;
            Assert.Equal(5, block.Count);
            Assert.All(block, node => Assert.Null(node.Parent));
        }

        [Fact]
        public void ReplacingVariablesInAssignmentShouldGetRidOfOldOnes()
        {
            var var1 = new DummyVariable("1");
            var ast = ConstructAst(new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Set(1, var1),
                DummyInstruction.Ret(2),
            });

            Assert.Single(ast.Nodes);
            var block = ast.Entrypoint.Contents.Instructions;
            Assert.Equal(3, block.Count);
            Assert.All(block, node => Assert.Null(node.Parent));

            var var2 = new DummyVariable("2");
            var var3 = new DummyVariable("3");
            var asmt = Assert.IsType<AssignmentStatement<DummyInstruction>>(block[1]);
            Assert.Equal(2, asmt.WithVariables(var2, var3).Variables.Count);
            Assert.Equal(asmt.Variables[0], var2);
            Assert.Equal(asmt.Variables[1], var3);
        }

        [Fact]
        public void ReplacingExpressionInAssignmentShouldChangeParent()
        {
            var var1 = new DummyVariable("1");
            var ast = ConstructAst(new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Set(1, var1),
                DummyInstruction.Ret(2)
            });

            Assert.Single(ast.Nodes);
            var block = ast.Entrypoint.Contents.Instructions;
            Assert.Equal(3, block.Count);
            Assert.All(block, node => Assert.Null(node.Parent));

            var expr = new InstructionExpression<DummyInstruction>(DummyInstruction.Jmp(0, 69),
                Array.Empty<Expression<DummyInstruction>>());
            var asmt = Assert.IsType<AssignmentStatement<DummyInstruction>>(block[1]);
            Assert.Equal(expr, asmt.WithExpression(expr).Expression);
            Assert.Equal(asmt, expr.Parent);
        }

        [Fact]
        public void ReplacingExpressionInStatementShouldChangeParent()
        {
            var ast = ConstructAst(new[]
            {
                DummyInstruction.Push(0, 2),
                DummyInstruction.Pop(1, 2),
                DummyInstruction.Ret(2)
            });

            Assert.Single(ast.Nodes);
            var block = ast.Entrypoint.Contents.Instructions;
            Assert.Equal(3, block.Count);
            Assert.All(block, node => Assert.Null(node.Parent));

            var expr = new InstructionExpression<DummyInstruction>(DummyInstruction.Jmp(0, 69),
                Array.Empty<Expression<DummyInstruction>>());
            var stmt = Assert.IsType<ExpressionStatement<DummyInstruction>>(block[1]);
            Assert.Equal(expr, stmt.WithExpression(expr).Expression);
            Assert.Equal(stmt, expr.Parent);
        }

        [Fact]
        public void ReplacingInstructionInInstructionExpression()
        {
            var ast = ConstructAst(new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Pop(1, 1),
                DummyInstruction.Ret(2)
            });

            Assert.Single(ast.Nodes);
            var block = ast.Entrypoint.Contents.Instructions;
            Assert.Equal(3, block.Count);
            Assert.All(block, node => Assert.Null(node.Parent));

            var instruction = DummyInstruction.Jmp(0, 69);
            var asmt = Assert.IsType<AssignmentStatement<DummyInstruction>>(block[0]);
            var expr = Assert.IsType<InstructionExpression<DummyInstruction>>(asmt.Expression);

            Assert.Equal(instruction, expr.WithInstruction(instruction).Instruction);
        }

        [Fact]
        public void ReplacingArgumentsInInstructionExpression()
        {
            var ast = ConstructAst(new[]
            {
                DummyInstruction.Push(0, 1),
                DummyInstruction.Pop(1, 1),
                DummyInstruction.Ret(2)
            });

            Assert.Single(ast.Nodes);
            var block = ast.Entrypoint.Contents.Instructions;
            Assert.Equal(3, block.Count);
            Assert.All(block, node => Assert.Null(node.Parent));

            var stmt = Assert.IsType<ExpressionStatement<DummyInstruction>>(block[1]);
            var expr = Assert.IsType<InstructionExpression<DummyInstruction>>(stmt.Expression);
            
            Assert.Single(expr.Arguments);
            var original = Assert.IsType<VariableExpression<DummyInstruction>>(expr.Arguments[0]);
            var newOne = new VariableExpression<DummyInstruction>(new DummyVariable("dummy"));

            expr.WithArguments(original, newOne);
            
            Assert.Equal(2, expr.Arguments.Count);
            Assert.Equal(original, expr.Arguments[0]);
            Assert.Equal(newOne, expr.Arguments[1]);
        }
    }
}