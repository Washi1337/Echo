using Echo.Ast.Pattern;
using Xunit;

namespace Echo.Ast.Tests.Pattern
{
    public class LiteralPatternTest
    {
        [Fact]
        public void SameValueShouldMatch()
        {
            var pattern = Pattern<int>.Literal(123);
            var result = pattern.Match(123);
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void DifferentValueShouldNotMatch()
        {
            var pattern = Pattern<int>.Literal(123);
            var result = pattern.Match(456);
            Assert.False(result.IsSuccess);
        }
    }
}