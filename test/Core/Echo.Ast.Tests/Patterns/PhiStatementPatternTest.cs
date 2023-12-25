using System.Collections.Generic;
using Echo.Ast.Patterns;
using Echo.Code;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class PhiStatementPattern
    {
        [Fact]
        public void AnyPhiWithAnyVariables()
        {
            var statement = new PhiStatement<int>(new DummyVariable("phi1"), new VariableExpression<int>[]
            {
                new(new DummyVariable("v1")),
                new(new DummyVariable("v2")),
                new(new DummyVariable("v3")),
                new(new DummyVariable("v4")),
            });

            var pattern = StatementPattern.Phi<int>();

            Assert.True(pattern.Matches(statement));
        }
        
        [Fact]
        public void AnyPhiWithSpecificTargetPattern()
        {
            var statement = new PhiStatement<int>(new DummyVariable("phi1"), new VariableExpression<int>[]
            {
                new(new DummyVariable("v1")),
                new(new DummyVariable("v2")),
                new(new DummyVariable("v3")),
                new(new DummyVariable("v4")),
            });
            
            var group = new CaptureGroup<IVariable>("phiGroup");

            var pattern = StatementPattern.Phi<int>()
                .WithTarget(Pattern.Any<IVariable>().CaptureAs(group));

            var result = pattern.Match(statement);
            Assert.True(result.IsSuccess);
            Assert.Contains(group, result.GetCaptureGroups());
            Assert.Contains(statement.Representative, result.GetCaptures(group));
        }
        
        [Fact]
        public void AnyPhiWithFixedVariables()
        {
            var statement = new PhiStatement<int>(new DummyVariable("phi1"), new VariableExpression<int>[]
            {
                new(new DummyVariable("v1")),
                new(new DummyVariable("v2")),
                new(new DummyVariable("v3")),
            });

            var pattern = StatementPattern
                .Phi<int>()
                .WithSources(
                    Pattern.Any<VariableExpression<int>>(),
                    Pattern.Any<VariableExpression<int>>(),
                    Pattern.Any<VariableExpression<int>>()
                );

            Assert.True(pattern.Matches(statement));

            statement.Sources.Add(new VariableExpression<int>(new DummyVariable("v4")));

            Assert.False(pattern.Matches(statement));
        }
        
    }
}