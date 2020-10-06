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
            Assert.True(pattern.Matches(123));
        }
        
        [Fact]
        public void DifferentValueShouldNotMatch()
        {
            var pattern = Pattern.Literal(123);
            Assert.False(pattern.Matches(456));
        }
    }
}