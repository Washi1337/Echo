using System.Collections.Generic;
using Echo.Platforms.DummyPlatform.Values;
using Echo.Symbolic.Values;
using Xunit;

namespace Echo.Symbolic.Tests.Values
{
    public class SymbolicValueTest
    {
        [Fact]
        public void MergeExpansion()
        {
            var sources = new IDataSource[]
            {
                new DummyDataSource(0), new DummyDataSource(1), new DummyDataSource(2)
            };
            
            var value1 = new SymbolicValue(sources[0]);
            var value2 = new SymbolicValue(sources[1], sources[2]);
            
            Assert.True(value1.MergeWith(value2));
            Assert.Equal(new HashSet<IDataSource>(sources), value1.DataSources);
        }
        
        [Fact]
        public void MergeNoChange()
        {
            var sources = new IDataSource[]
            {
                new DummyDataSource(0), new DummyDataSource(1)
            };
            
            var value1 = new SymbolicValue(sources[0], sources[1]);
            var value2 = new SymbolicValue(sources[1], sources[0]);
            
            Assert.False(value1.MergeWith(value2));
            Assert.Equal(new HashSet<IDataSource>(sources), value1.DataSources);
        }
    }
}