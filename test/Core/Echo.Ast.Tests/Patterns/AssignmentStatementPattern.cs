using System;
using Echo.Ast.Patterns;
using Echo.Core.Code;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class AssignmentStatementPattern
    {
        [Fact]
        public void AnyVariableAndAnyExpression()
        {
            var statement = new AssignmentStatement<int>(0,
                new InstructionExpression<int>(1, 1, ArraySegment<ExpressionBase<int>>.Empty), 
                new IVariable[]
                {
                    new AstVariable("var1"),
                });
            
            var pattern = StatementPattern.Assignment<int>();

            var result = pattern.Match(statement);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void AnyVariableWithSpecificExpression()
        {
            var group = new CaptureGroup("group");
            
            var statement = new AssignmentStatement<int>(0,
                new InstructionExpression<int>(1, 1, ArraySegment<ExpressionBase<int>>.Empty), 
                new IVariable[]
                {
                    new AstVariable("var1"),
                });

            var pattern = StatementPattern
                .Assignment<int>()
                .WithExpression(Pattern.Any<ExpressionBase<int>>().CaptureAs(group));

            var result = pattern.Match(statement);
            Assert.True(result.IsSuccess);
            Assert.Contains(group, result.Captures);
            Assert.Contains(statement.Expression, result.Captures[group]);
        }

        [Fact]
        public void SpecificVariable()
        {
            var group = new CaptureGroup("group");
            
            var statement = new AssignmentStatement<int>(0,
                new InstructionExpression<int>(1, 1, ArraySegment<ExpressionBase<int>>.Empty), 
                new IVariable[]
                {
                    new AstVariable("var1"),
                });

            var pattern = StatementPattern
                .Assignment<int>()
                .WithVariables(Pattern.Any<IVariable>().CaptureAs(group));

            var result = pattern.Match(statement);
            Assert.True(result.IsSuccess);
            Assert.Contains(group, result.Captures);
            Assert.Contains(statement.Variables[0], result.Captures[group]);
        }
    }
}