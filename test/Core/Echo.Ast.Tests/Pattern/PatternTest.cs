using Echo.Ast.Pattern;
using Xunit;

namespace Echo.Ast.Tests.Pattern
{
    public class PatternTest
    {
        private readonly CaptureGroup _captureGroup = new CaptureGroup("MyCapture");
        
        [Fact]
        public void NonCapturedAnyPatternShouldMatchAndExtractValue()
        {
            var pattern = Pattern<object>.Any();
            
            var myObject = new object();
            var result = pattern.Match(myObject);
            
            Assert.True(result.IsSuccess);
            Assert.DoesNotContain(_captureGroup, result.Captures);
        }

        [Fact]
        public void CapturedAnyPatternShouldMatchAndExtractValue()
        {
            var pattern = Pattern<object>.Any()
                .Capture(_captureGroup);
            
            var myObject = new object();
            var result = pattern.Match(myObject);
            
            Assert.True(result.IsSuccess);
            Assert.Contains(_captureGroup, result.Captures);
            Assert.Contains(myObject, result.Captures[_captureGroup]);
        }
    }
}