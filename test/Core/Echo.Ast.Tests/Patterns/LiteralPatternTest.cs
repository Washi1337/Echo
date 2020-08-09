using Echo.Ast.Patterns;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class LiteralPatternTest
    {
        [Fact]
        public void SameValueShouldMatch()
        {
            var pattern = Pattern.Literal(123);
            var result = pattern.Match(123);
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void DifferentValueShouldNotMatch()
        {
            var pattern = Pattern.Literal(123);
            var result = pattern.Match(456);
            Assert.False(result.IsSuccess);
        }
    }
}