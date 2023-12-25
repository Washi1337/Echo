using Echo.Ast.Patterns;
using Echo.Code;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class AssignmentStatementPattern
    {
        [Fact]
        public void AnyVariableAndAnyExpression()
        {
            var statement = Statement.Assignment(
                new DummyVariable("var1"),
                Expression.Instruction(1)
            );
            
            var pattern = StatementPattern.Assignment<int>();

            Assert.True(pattern.Matches(statement));
        }

        [Fact]
        public void AnyVariableWithSpecificExpression()
        {
            var statement = Statement.Assignment(
                new DummyVariable("var1"),
                Expression.Instruction(1)
            );
            
            var group = new CaptureGroup<Expression<int>>("group");
            var pattern = StatementPattern
                .Assignment<int>()
                .WithExpression(Pattern.Any<Expression<int>>().CaptureAs(group));

            var result = pattern.Match(statement);
            Assert.True(result.IsSuccess);
            Assert.Contains(group, result.GetCaptureGroups());
            Assert.Contains(statement.Expression, result.GetCaptures(group));
        }

        [Fact]
        public void SpecificVariable()
        {
            var group = new CaptureGroup<IVariable>("group");
            
            var statement = Statement.Assignment(
                new DummyVariable("var1"),
                Expression.Instruction(1)
            );

            var pattern = StatementPattern
                .Assignment<int>()
                .WithVariables(Pattern.Any<IVariable>().CaptureAs(group));

            var result = pattern.Match(statement);
            Assert.True(result.IsSuccess);
            Assert.Contains(group, result.GetCaptureGroups());
            Assert.Contains(statement.Variables[0], result.GetCaptures(group));
        }
    }
}