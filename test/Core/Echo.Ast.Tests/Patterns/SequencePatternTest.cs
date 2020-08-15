using System;
using Echo.Ast.Patterns;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class SequencePatternTest
    {
        [Fact]
        public void EmptySequenceShouldOnlyMatchOnEmptyCollections()
        {
            var pattern = new SequencePattern<int>();
            Assert.True(pattern.Match(ArraySegment<int>.Empty).IsSuccess);
            Assert.False(pattern.Match(new[] {1, 2, 3}).IsSuccess);
        }

        [Fact]
        public void SingleElementShouldMatchOnSameSingleElementList()
        {
            var pattern = new SequencePattern<int>(Pattern.Literal(1));
            Assert.True(pattern.Match(new[] {1}).IsSuccess);
            Assert.False(pattern.Match(new[] {2}).IsSuccess);
            Assert.False(pattern.Match(ArraySegment<int>.Empty).IsSuccess);
            Assert.False(pattern.Match(new[] {1, 2}).IsSuccess);
        }

        [Fact]
        public void TwoElementsShouldMatchInOrder()
        {
            var pattern = Pattern.Literal(1) + Pattern.Literal(2);
            
            Assert.True(pattern.Match(new[] {1, 2}).IsSuccess);
            Assert.False(pattern.Match(new[] {2, 1}).IsSuccess);
        }

        [Fact]
        public void MultipleElementsShouldMatchInOrder()
        {
            var pattern =
                Pattern.Literal(1)
                + Pattern.Literal(2)
                + Pattern.Literal(3)
                + Pattern.Literal(4)
                + Pattern.Literal(5);
            
            Assert.True(pattern.Match(new[] {1, 2, 3, 4, 5}).IsSuccess);
            Assert.False(pattern.Match(new[] {2, 1, 3, 5, 1}).IsSuccess);
            Assert.False(pattern.Match(new[] {1, 2, 3}).IsSuccess);
        }
    }
}