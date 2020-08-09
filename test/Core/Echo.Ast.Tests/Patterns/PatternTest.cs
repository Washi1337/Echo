using Echo.Ast.Patterns;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class PatternTest
    {
        private readonly CaptureGroup _captureGroup = new CaptureGroup("MyCapture");
        
        [Fact]
        public void NonCapturedAnyPatternShouldMatchAndExtractValue()
        {
            var pattern = Pattern.Any<object>();
            
            var myObject = new object();
            var result = pattern.Match(myObject);
            
            Assert.True(result.IsSuccess);
            Assert.DoesNotContain(_captureGroup, result.Captures);
        }

        [Fact]
        public void CapturedAnyPatternShouldMatchAndExtractValue()
        {
            var pattern = Pattern.Any<object>()
                .Capture(_captureGroup);
            
            var myObject = new object();
            var result = pattern.Match(myObject);
            
            Assert.True(result.IsSuccess);
            Assert.Contains(_captureGroup, result.Captures);
            Assert.Contains(myObject, result.Captures[_captureGroup]);
        }
    }
}