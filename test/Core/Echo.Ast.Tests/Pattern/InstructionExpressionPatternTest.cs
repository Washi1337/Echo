using System.Collections.Generic;
using System.Collections.Immutable;
using Echo.Ast.Pattern;
using Xunit;

namespace Echo.Ast.Tests.Pattern
{
    public class InstructionExpressionPatternTest
    {
        [Fact]
        public void SameInstructionWithZeroArgumentsShouldMatch()
        {
            var pattern = ExpressionPattern<int>.Instruction(1234);
            var input = new AstInstructionExpression<int>(0, 1234, ImmutableArray<AstExpressionBase<int>>.Empty);
            var result = pattern.Match(input);
            
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void DifferentInstructionWithZeroArgumentsShouldNotMatch()
        {
            var pattern = ExpressionPattern<int>.Instruction(1234);
            var input = new AstInstructionExpression<int>(0, 5678, ImmutableArray<AstExpressionBase<int>>.Empty);
            var result = pattern.Match(input);
            
            Assert.False(result.IsSuccess);
        }
        
        [Theory]
        [InlineData(false, 0)]
        [InlineData(false, 3)]
        [InlineData(true, 0)]
        [InlineData(true, 3)]
        public void InstructionWithAnyArgumentsShouldMatchIfInstructionIsEqual(bool sameInstruction, int argumentCount)
        {
            var pattern = ExpressionPattern<int>
                .Instruction(1234)
                .WithAnyArguments();

            var arguments = new List<AstExpressionBase<int>>(argumentCount);
            for (int i = 0; i < argumentCount; i++)
                arguments.Add(new AstInstructionExpression<int>(i, 0, ImmutableArray<AstExpressionBase<int>>.Empty));

            var input = new AstInstructionExpression<int>(argumentCount, sameInstruction ? 1234 : 5678, arguments);
            
            var result = pattern.Match(input);

            Assert.Equal(sameInstruction, result.IsSuccess);
        }
        
        [Fact]
        public void SameInstructionWithMatchingArgumentsShouldMatch()
        {
            var pattern = ExpressionPattern<int>
                .Instruction(1234)
                .WithArguments(ExpressionPattern<int>.Any(), ExpressionPattern<int>.Any());

            var arguments = new List<AstExpressionBase<int>>(2)
            {
                new AstInstructionExpression<int>(0, 0, ImmutableArray<AstExpressionBase<int>>.Empty),
                new AstInstructionExpression<int>(1, 1, ImmutableArray<AstExpressionBase<int>>.Empty),
            };

            var input = new AstInstructionExpression<int>(2, 1234, arguments);
            
            var result = pattern.Match(input);

            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void DifferentInstructionWithMatchingArgumentsShouldNotMatch()
        {
            var pattern = ExpressionPattern<int>
                .Instruction(1234)
                .WithArguments(ExpressionPattern<int>.Any(), ExpressionPattern<int>.Any());

            var arguments = new List<AstExpressionBase<int>>(2)
            {
                new AstInstructionExpression<int>(0, 0, ImmutableArray<AstExpressionBase<int>>.Empty),
                new AstInstructionExpression<int>(1, 1, ImmutableArray<AstExpressionBase<int>>.Empty),
            };

            var input = new AstInstructionExpression<int>(2, 5678, arguments);
            
            var result = pattern.Match(input);

            Assert.False(result.IsSuccess);
        }
        
        [Fact]
        public void SameInstructionWithNonMatchingArgumentsShouldNotMatch()
        {
            var pattern = ExpressionPattern<int>
                .Instruction(1234)
                .WithArguments(ExpressionPattern<int>.Instruction(5678), ExpressionPattern<int>.Any());

            var arguments = new List<AstExpressionBase<int>>(2)
            {
                new AstInstructionExpression<int>(0, 0, ImmutableArray<AstExpressionBase<int>>.Empty),
                new AstInstructionExpression<int>(1, 1, ImmutableArray<AstExpressionBase<int>>.Empty),
            };

            var input = new AstInstructionExpression<int>(2, 1234, arguments);
            
            var result = pattern.Match(input);

            Assert.False(result.IsSuccess);
        }
        
        [Fact]
        public void SameInstructionWithDifferentArgumentCountShouldNotMatch()
        {
            var pattern = ExpressionPattern<int>
                .Instruction(1234)
                .WithArguments(ExpressionPattern<int>.Any(), ExpressionPattern<int>.Any());

            var arguments = new List<AstExpressionBase<int>>(2)
            {
                new AstInstructionExpression<int>(0, 0, ImmutableArray<AstExpressionBase<int>>.Empty),
            };

            var input = new AstInstructionExpression<int>(2, 1234, arguments);
            
            var result = pattern.Match(input);

            Assert.False(result.IsSuccess);
        }
        
    }
}