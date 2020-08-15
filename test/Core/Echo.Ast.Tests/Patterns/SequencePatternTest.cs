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
            Assert.True(pattern.Matches(ArraySegment<int>.Empty));
            Assert.False(pattern.Matches(new[] {1, 2, 3}));
        }

        [Fact]
        public void SingleElementShouldMatchOnSameSingleElementList()
        {
            var pattern = new SequencePattern<int>(Pattern.Literal(1));
            Assert.True(pattern.Matches(new[] {1}));
            Assert.False(pattern.Matches(new[] {2}));
            Assert.False(pattern.Matches(ArraySegment<int>.Empty));
            Assert.False(pattern.Matches(new[] {1, 2}));
        }

        [Fact]
        public void TwoElementsShouldMatchInOrder()
        {
            var pattern = Pattern.Literal(1) + Pattern.Literal(2);
            
            Assert.True(pattern.Matches(new[] {1, 2}));
            Assert.False(pattern.Matches(new[] {2, 1}));
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
            
            Assert.True(pattern.Matches(new[] {1, 2, 3, 4, 5}));
            Assert.False(pattern.Matches(new[] {2, 1, 3, 5, 1}));
            Assert.False(pattern.Matches(new[] {1, 2, 3}));
        }
    }
}