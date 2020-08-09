using Echo.Ast.Pattern;
using Xunit;

namespace Echo.Ast.Tests.Pattern
{
    public class OrPatternTest
    {
        [Fact]
        public void FirstOfTwoOptionsMatchShouldResultInMatch()
        {
            var pattern = Pattern<int>.Literal(1) | Pattern<int>.Literal(2);
            var result = pattern.Match(1);
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void SecondOfTwoOptionsMatchShouldResultInMatch()
        {
            var pattern = Pattern<int>.Literal(1) | Pattern<int>.Literal(2);
            var result = pattern.Match(2);
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void NeitherOfTwoOptionsMatchShouldResultInNoMatch()
        {
            var pattern = Pattern<int>.Literal(1) | Pattern<int>.Literal(2);
            var result = pattern.Match(3);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void LeftHandSideOrPatternShouldBeFlattened()
        {
            var pattern1 = Pattern<int>.Literal(1);
            var pattern2 = Pattern<int>.Literal(2);
            var pattern3 = Pattern<int>.Literal(3);

            var combined = pattern1 | pattern2;
            combined = combined | pattern3;

            Assert.Equal(new[] {pattern1, pattern2, pattern3}, combined.Options);
        }

        [Fact]
        public void RightHandSideOrPatternShouldBeFlattened()
        {
            var pattern1 = Pattern<int>.Literal(1);
            var pattern2 = Pattern<int>.Literal(2);
            var pattern3 = Pattern<int>.Literal(3);

            var combined = pattern2 | pattern3;
            combined = pattern1 | combined;

            Assert.Equal(new[] {pattern1, pattern2, pattern3}, combined.Options);
        }
        
    }
}