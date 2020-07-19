using Xunit;

namespace Echo.Core.Tests
{
    public class TrileanTest
    {
        [Theory]
        [InlineData(TrileanValue.False, TrileanValue.False, TrileanValue.False)]
        [InlineData(TrileanValue.False, TrileanValue.True, TrileanValue.True)]
        [InlineData(TrileanValue.False, TrileanValue.Unknown, TrileanValue.Unknown)]
        [InlineData(TrileanValue.True, TrileanValue.False, TrileanValue.True)]
        [InlineData(TrileanValue.True, TrileanValue.True, TrileanValue.True)]
        [InlineData(TrileanValue.True, TrileanValue.Unknown, TrileanValue.True)]
        [InlineData(TrileanValue.Unknown, TrileanValue.False, TrileanValue.Unknown)]
        [InlineData(TrileanValue.Unknown, TrileanValue.True, TrileanValue.True)]
        [InlineData(TrileanValue.Unknown, TrileanValue.Unknown, TrileanValue.Unknown)]
        public void OrTest(TrileanValue a, TrileanValue b, TrileanValue expected)
        {
            Trilean x = a;
            Trilean y = b;
            Trilean z = expected;
            
            Assert.Equal(z, x.Or(y));
            Assert.Equal(z, x | y);
        }
    }
}