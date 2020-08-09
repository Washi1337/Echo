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
            var result = pattern.Match(new object());
            Assert.True(result.IsSuccess);
        }
    }
}