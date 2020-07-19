using Xunit;
using static Echo.Core.TrileanValue;

namespace Echo.Core.Tests
{
    public class TrileanTest
    {
        [Theory]
        [InlineData(False, False, False)]
        [InlineData(False, True, False)]
        [InlineData(False, Unknown, False)]
        [InlineData(True, False, False)]
        [InlineData(True, True, True)]
        [InlineData(True, Unknown, Unknown)]
        [InlineData(Unknown, False, False)]
        [InlineData(Unknown, True, Unknown)]
        [InlineData(Unknown, Unknown, Unknown)]
        public void AndTest(TrileanValue a, TrileanValue b, TrileanValue expected)
        {
            Trilean x = a;
            Trilean y = b;
            Trilean z = expected;
            
            Assert.Equal(z, x.And(y));
            Assert.Equal(z, x & y);
        }
        
        [Theory]
        [InlineData(False, False, False)]
        [InlineData(False, True, True)]
        [InlineData(False, Unknown, Unknown)]
        [InlineData(True, False, True)]
        [InlineData(True, True, True)]
        [InlineData(True, Unknown, True)]
        [InlineData(Unknown, False, Unknown)]
        [InlineData(Unknown, True, True)]
        [InlineData(Unknown, Unknown, Unknown)]
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