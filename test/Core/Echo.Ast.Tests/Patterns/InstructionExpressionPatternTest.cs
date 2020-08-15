using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Echo.Ast.Patterns;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class InstructionExpressionPatternTest
    {
        [Fact]
        public void SameInstructionWithZeroArgumentsShouldMatch()
        {
            var pattern = ExpressionPattern.InstructionLiteral(1234);
            var input = new InstructionExpression<int>(0, 1234, ImmutableArray<ExpressionBase<int>>.Empty);

            Assert.True(pattern.Matches(input));
        }
        
        [Fact]
        public void DifferentInstructionWithZeroArgumentsShouldNotMatch()
        {
            var pattern = ExpressionPattern.InstructionLiteral(1234);
            var input = new InstructionExpression<int>(0, 5678, ImmutableArray<ExpressionBase<int>>.Empty);

            Assert.False(pattern.Matches(input));
        }
        
        [Theory]
        [InlineData(false, 0)]
        [InlineData(false, 3)]
        [InlineData(true, 0)]
        [InlineData(true, 3)]
        public void InstructionWithAnyArgumentsShouldMatchIfInstructionIsEqual(bool sameInstruction, int argumentCount)
        {
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithAnyArguments();

            var arguments = new List<ExpressionBase<int>>(argumentCount);
            for (int i = 0; i < argumentCount; i++)
                arguments.Add(new InstructionExpression<int>(i, 0, ImmutableArray<ExpressionBase<int>>.Empty));

            var input = new InstructionExpression<int>(argumentCount, sameInstruction ? 1234 : 5678, arguments);
            
            var result = pattern.Match(input);

            Assert.Equal(sameInstruction, result.IsSuccess);
        }
        
        [Fact]
        public void SameInstructionWithMatchingArgumentsShouldMatch()
        {
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(ExpressionPattern.Any<int>(), ExpressionPattern.Any<int>());

            var arguments = new List<ExpressionBase<int>>(2)
            {
                new InstructionExpression<int>(0, 0, ImmutableArray<ExpressionBase<int>>.Empty),
                new InstructionExpression<int>(1, 1, ImmutableArray<ExpressionBase<int>>.Empty),
            };

            var input = new InstructionExpression<int>(2, 1234, arguments);

            Assert.True(pattern.Matches(input));
        }
        
        [Fact]
        public void DifferentInstructionWithMatchingArgumentsShouldNotMatch()
        {
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(ExpressionPattern.Any<int>(), ExpressionPattern.Any<int>());

            var arguments = new List<ExpressionBase<int>>(2)
            {
                new InstructionExpression<int>(0, 0, ImmutableArray<ExpressionBase<int>>.Empty),
                new InstructionExpression<int>(1, 1, ImmutableArray<ExpressionBase<int>>.Empty),
            };

            var input = new InstructionExpression<int>(2, 5678, arguments);

            Assert.False(pattern.Matches(input));
        }
        
        [Fact]
        public void SameInstructionWithNonMatchingArgumentsShouldNotMatch()
        {
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(ExpressionPattern.InstructionLiteral(5678), ExpressionPattern.Any<int>());

            var arguments = new List<ExpressionBase<int>>(2)
            {
                new InstructionExpression<int>(0, 0, ImmutableArray<ExpressionBase<int>>.Empty),
                new InstructionExpression<int>(1, 1, ImmutableArray<ExpressionBase<int>>.Empty),
            };

            var input = new InstructionExpression<int>(2, 1234, arguments);

            Assert.False(pattern.Matches(input));
        }
        
        [Fact]
        public void SameInstructionWithDifferentArgumentCountShouldNotMatch()
        {
            var pattern = ExpressionPattern
                .InstructionLiteral(1234)
                .WithArguments(ExpressionPattern.Any<int>(), ExpressionPattern.Any<int>());

            var arguments = new List<ExpressionBase<int>>(2)
            {
                new InstructionExpression<int>(0, 0, ImmutableArray<ExpressionBase<int>>.Empty),
            };

            var input = new InstructionExpression<int>(2, 1234, arguments);

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

            var arguments = new List<ExpressionBase<int>>(2)
            {
                new InstructionExpression<int>(0, 5678, ImmutableArray<ExpressionBase<int>>.Empty),
                new InstructionExpression<int>(1, 1, ImmutableArray<ExpressionBase<int>>.Empty),
            };

            var input = new InstructionExpression<int>(2, 1234, arguments);

            Assert.True(pattern.Matches(input));
        }

    }
}