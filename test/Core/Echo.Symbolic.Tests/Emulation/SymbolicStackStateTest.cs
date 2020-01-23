using System.Collections.Generic;
using Echo.Core.Emulation;
using Echo.Platforms.DummyPlatform.Values;
using Echo.Symbolic.Emulation;
using Echo.Symbolic.Tests.Values;
using Echo.Symbolic.Values;
using Xunit;

namespace Echo.Symbolic.Tests.Emulation
{
    public class SymbolicStackStateTest
    {
        [Fact]
        public void MergeSingle()
        {
            var sources = new IDataSource[]
            {
                new DummyDataSource(0),
                new DummyDataSource(1)
            };
            
            var stack1 = new StackState<SymbolicValue>();
            var value1 = new SymbolicValue(sources[0]);
            stack1.Push(value1);
            
            var stack2 = new StackState<SymbolicValue>();
            var value2 = new SymbolicValue(sources[1]);
            stack2.Push(value2);
            
            Assert.True(stack1.MergeWith(stack2));
            Assert.Equal(new HashSet<IDataSource>(sources), stack1.Top.DataSources);
        }
        
        [Fact]
        public void MergeMultiple()
        {
            var sources = new[]
            {
                new IDataSource[]
                {
                    new DummyDataSource(0),
                    new DummyDataSource(1)
                },
                new IDataSource[]
                {
                    new DummyDataSource(2),
                    new DummyDataSource(3)
                },
                new IDataSource[]
                {
                    new DummyDataSource(4),
                    new DummyDataSource(5)
                }
            };
            
            var stack1 = new StackState<SymbolicValue>();
            var values1 = new[]
            {
                new SymbolicValue(sources[0][0]),
                new SymbolicValue(sources[1][0]),
                new SymbolicValue(sources[2][0]),
            };
            stack1.Push(values1);
            
            var stack2 = new StackState<SymbolicValue>();
            var values2 = new[]
            {
                new SymbolicValue(sources[0][1]),
                new SymbolicValue(sources[1][1]),
                new SymbolicValue(sources[2][1]),
            };
            stack2.Push(values2);
            
            Assert.True(stack1.MergeWith(stack2));

            int index = sources.Length - 1;
            foreach (var slot in stack1.GetAllStackSlots())
            {
                Assert.Equal(new HashSet<IDataSource>(sources[index]), slot.DataSources);
                index--;
            }
        }

        [Fact]
        public void MergeStackImbalance()
        {
            var stack1 = new StackState<SymbolicValue>();
            stack1.Push(new[]
            {
                new SymbolicValue(),
                new SymbolicValue(),
            });
            
            var stack2 = new StackState<SymbolicValue>();
            stack2.Push(new[]
            {
                new SymbolicValue(),
            });

            Assert.Throws<StackImbalanceException>(() => stack1.MergeWith(stack2));
        }
    }
}