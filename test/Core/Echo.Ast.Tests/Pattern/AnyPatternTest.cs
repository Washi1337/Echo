using Echo.Ast.Pattern;
using Xunit;

namespace Echo.Ast.Tests.Pattern
{
    public class AnyPatternTest
    {
        [Fact]
        public void AnyPatternShouldMatchAnyObject()
        {
            var pattern = Pattern<object>.Any();
            var result = pattern.Match(new object());
            Assert.True(result.IsSuccess);
        }
    }
}