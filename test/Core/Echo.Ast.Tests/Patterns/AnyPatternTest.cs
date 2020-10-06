using Echo.Ast.Patterns;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class AnyPatternTest
    {
        [Fact]
        public void AnyPatternShouldMatchAnyObject()
        {
            var pattern = Pattern.Any<object>();
            Assert.True(pattern.Matches(new object()));
        }
    }
}