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
            Assert.True(pattern.Matches(new VariableExpression<DummyInstruction>(0, variable)));
        }

        [Fact]
        public void SameSpecificVariableShouldMatch()
        {
            var variable = new DummyVariable("variable");
            var pattern = ExpressionPattern.Variable<DummyInstruction>(variable);
            Assert.True(pattern.Matches(new VariableExpression<DummyInstruction>(0, variable)));
        }

        [Fact]
        public void DifferentSpecificVariableShouldMatch()
        {
            var variable1 = new DummyVariable("variable1");
            var variable2 = new DummyVariable("variable2");
            
            var pattern = ExpressionPattern.Variable<DummyInstruction>(variable1);
            Assert.False(pattern.Matches(new VariableExpression<DummyInstruction>(0, variable2)));
        }
    }
}