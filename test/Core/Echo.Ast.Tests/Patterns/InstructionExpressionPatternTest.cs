using Echo.Ast.Patterns;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class InstructionExpressionPatternTest
    {
        [Fact]
        public void SameInstructionWithZeroArgumentsShouldMatch()
        {
            var input = Expression.Instruction(1234);
            var pattern = ExpressionPattern.InstructionLiteral(1234);

            Assert.True(pattern.Matches(input));
        }
        
        [Fact]
        public void DifferentInstructionWithZeroArgumentsShouldNotMatch()
        {
            var input = Expression.Instruction(5678);
            var pattern = ExpressionPattern.InstructionLiteral(1234);

            Assert.False(pattern.Matches(input));
        }
        
        [Theory]
        [InlineData(false, 0)]
        [InlineData(false, 3)]
        [InlineData(true, 0)]
        [InlineData(true, 3)]
        public void InstructionWithAnyArgumentsShouldMatchIfInstructionIsEqual(bool sameInstruction, int argumentCount)
        {
            var arguments = new Expression<int>[argumentCount];
            for (int i = 0; i < argumentCount; i++)
                arguments[i] = Expression.Instruction(i);

            var input = Expression.Instruction(sameInstruction ? 1234 : 5678, arguments);

            var result = ExpressionPattern
                .InstructionLiteral(1234)
                .WithAnyArguments()
                .Match(input);

            Assert.Equal(sameInstruction, result.IsSuccess);
        }
        
        [Fact]
        public void SameInstructionWithMatchingArgumentsShouldMatch()
        {
            var input = Expression
                .Instruction(1234)
                .WithArguments(
                    Expression.Instruction(0),
                    Expression.Instruction(1)
                );
            
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(ExpressionPattern.Any<int>(), ExpressionPattern.Any<int>());

            Assert.True(pattern.Matches(input));
        }
        
        [Fact]
        public void DifferentInstructionWithMatchingArgumentsShouldNotMatch()
        {
            var input = new InstructionExpression<int>(
                5678,
                Expression.Instruction(0),
                Expression.Instruction(1)
            );
            
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(ExpressionPattern.Any<int>(), ExpressionPattern.Any<int>());

            Assert.False(pattern.Matches(input));
        }
        
        [Fact]
        public void SameInstructionWithNonMatchingArgumentsShouldNotMatch()
        {
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(
                    ExpressionPattern.InstructionLiteral(5678), 
                    ExpressionPattern.Any<int>()
                );

            var input = Expression
                .Instruction(1234)
                .WithArguments(
                    Expression.Instruction(0),
                    Expression.Instruction(1)
                );

            Assert.False(pattern.Matches(input));
        }
        
        [Fact]
        public void SameInstructionWithDifferentArgumentCountShouldNotMatch()
        {
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(
                    ExpressionPattern.Any<int>(),
                    ExpressionPattern.Any<int>()
                );

            var input = Expression
                .Instruction(1234)
                .WithArguments(
                    Expression.Instruction(0)
                );

            Assert.False(pattern.Matches(input));
        }

        [Fact]
        public void SameInstructionWithComplexMatchingArgumentsShouldNotMatch()
        {
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(
                    ExpressionPattern.InstructionLiteral(1234) | ExpressionPattern.InstructionLiteral(5678),
                    ExpressionPattern.Any<int>());

            var input = Expression
                .Instruction(1234)
                .WithArguments(
                    Expression.Instruction(5678),
                    Expression.Instruction(1)
                );

            Assert.True(pattern.Matches(input));
        }

    }
}