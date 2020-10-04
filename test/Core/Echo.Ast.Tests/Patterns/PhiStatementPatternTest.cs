using System.Collections.Generic;
using Echo.Ast.Patterns;
using Echo.Core.Code;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class PhiStatementPattern
    {
        [Fact]
        public void AnyPhiWithAnyVariables()
        {
            var statement = new PhiStatement<int>(new List<VariableExpression<int>>
            {
                new VariableExpression<int>(new AstVariable("v1")),
                new VariableExpression<int>(new AstVariable("v2")),
                new VariableExpression<int>(new AstVariable("v3")),
                new VariableExpression<int>(new AstVariable("v4")),
            }, new AstVariable("phi1"));

            var pattern = StatementPattern.Phi<int>();

            Assert.True(pattern.Matches(statement));
        }
        
        [Fact]
        public void AnyPhiWithSpecificTargetPattern()
        {
            var group = new CaptureGroup("phiGroup");
            
            var statement = new PhiStatement<int>(new List<VariableExpression<int>>
            {
                new VariableExpression<int>(new AstVariable("v1")),
                new VariableExpression<int>(new AstVariable("v2")),
                new VariableExpression<int>(new AstVariable("v3")),
                new VariableExpression<int>(new AstVariable("v4")),
            }, new AstVariable("phi1"));

            var pattern = StatementPattern.Phi<int>()
                .WithTarget(Pattern.Any<IVariable>().CaptureAs(group));

            var result = pattern.Match(statement);
            Assert.True(result.IsSuccess);
            Assert.Contains(group, result.Captures);
            Assert.Contains(statement.Target, result.Captures[group]);
        }
        
        [Fact]
        public void AnyPhiWithFixedVariables()
        {
            var statement = new PhiStatement<int>(new List<VariableExpression<int>>
            {
                new VariableExpression<int>(new AstVariable("v1")),
                new VariableExpression<int>(new AstVariable("v2")),
                new VariableExpression<int>(new AstVariable("v3")),
            }, new AstVariable("phi1"));

            var pattern = StatementPattern.Phi<int>()
                .WithSources(
                    Pattern.Any<VariableExpression<int>>(),
                    Pattern.Any<VariableExpression<int>>(),
                    Pattern.Any<VariableExpression<int>>());

            Assert.True(pattern.Matches(statement));

            statement.Sources.Add(new VariableExpression<int>(new AstVariable("v4")));

            Assert.False(pattern.Matches(statement));
        }
        
        
    }
}