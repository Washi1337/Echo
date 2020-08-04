using System.Collections.Generic;
using Echo.DataFlow.Values;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.DataFlow.Tests.Values
{
    public class SymbolicValueTest
    {
        private static DataFlowNode<DummyInstruction> CreateDummyNode(long id)
        {
            return new DataFlowNode<DummyInstruction>(id, DummyInstruction.Op(id, 0, 1));
        }
        
        [Fact]
        public void MergeExpansion()
        {
            var sources = new[]
            {
                CreateDummyNode(0),
                CreateDummyNode(1),
                CreateDummyNode(2),
            };
            
            var value1 = new SymbolicValue<DummyInstruction>(sources[0]);
            var value2 = new SymbolicValue<DummyInstruction>(new[] {sources[1], sources[2]});
            
            Assert.True(value1.MergeWith(value2));
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(sources), value1.GetNodes());
        }

        [Fact]
        public void MergeNoChange()
        {
            var sources = new[]
            {
                CreateDummyNode(0),
                CreateDummyNode(1)
            };

            var value1 = new SymbolicValue<DummyInstruction>(new[] {sources[0], sources[1]});
            var value2 = new SymbolicValue<DummyInstruction>(new[] {sources[1], sources[0]});

            Assert.False(value1.MergeWith(value2));
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(sources), value1.GetNodes());
        }
    }
}