using Echo.Ast.Patterns;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class VariableExpressionPatternTest
    {
        [Fact]
        public void AnyVariableExpressionShouldMatch()
        {
            var variable = new DummyVariable("variable");
            var pattern = ExpressionPattern.Variable<DummyInstruction>();
            var result = pattern.Match(new VariableExpression<DummyInstruction>(0, variable));
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void SameSpecificVariableShouldMatch()
        {
            var variable = new DummyVariable("variable");
            var pattern = ExpressionPattern.Variable<DummyInstruction>(variable);
            var result = pattern.Match(new VariableExpression<DummyInstruction>(0, variable));
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void DifferentSpecificVariableShouldMatch()
        {
            var variable1 = new DummyVariable("variable1");
            var variable2 = new DummyVariable("variable2");
            
            var pattern = ExpressionPattern.Variable<DummyInstruction>(variable1);
            var result = pattern.Match(new VariableExpression<DummyInstruction>(0, variable2));
            Assert.False(result.IsSuccess);
        }
    }
}