using Echo.Ast.Patterns;
using Xunit;

namespace Echo.Ast.Tests.Patterns
{
    public class PatternTest
    {
        private readonly CaptureGroup<object> _captureGroup = new("MyCapture");
        
        [Fact]
        public void NonCapturedAnyPatternShouldMatchAndExtractValue()
        {
            var pattern = Pattern.Any<object>();
            
            object myObject = new();
            var result = pattern.Match(myObject);
            
            Assert.True(result.IsSuccess);
            Assert.DoesNotContain(_captureGroup, result.GetCaptures(_captureGroup));
        }

        [Fact]
        public void CapturedAnyPatternShouldMatchAndExtractValue()
        {
            var pattern = Pattern.Any<object>()
                .CaptureAs(_captureGroup);
            
            object myObject = new();
            var result = pattern.Match(myObject);
            
            Assert.True(result.IsSuccess);
            Assert.Contains(_captureGroup, result.GetCaptureGroups());
            Assert.Contains(myObject, result.GetCaptures(_captureGroup));
        }
    }
}