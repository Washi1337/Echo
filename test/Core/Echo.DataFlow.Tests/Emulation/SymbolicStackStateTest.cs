using System.Collections.Generic;
using Echo.Core.Emulation;
using Echo.DataFlow.Emulation;
using Echo.DataFlow.Values;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.DataFlow.Tests.Emulation
{
    public class SymbolicStackStateTest
    {
        private static DataFlowNode<DummyInstruction> CreateDummyNode(long id)
        {
            return new DataFlowNode<DummyInstruction>(id, DummyInstruction.Op(id, 0, 1));
        }
        
        [Fact]
        public void MergeSingle()
        {
            var sources = new[]
            {
                CreateDummyNode(0), CreateDummyNode(1),
            };
            
            var stack1 = new StackState<SymbolicValue<DummyInstruction>>();
            var value1 = new SymbolicValue<DummyInstruction>(sources[0]);
            stack1.Push(value1);
            
            var stack2 = new StackState<SymbolicValue<DummyInstruction>>();
            var value2 = new SymbolicValue<DummyInstruction>(sources[1]);
            stack2.Push(value2);
            
            Assert.True(stack1.MergeWith(stack2));
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(sources), stack1.Top.GetNodes());
        }
        
        [Fact]
        public void MergeMultiple()
        {
            var sources = new[]
            {
                new[]
                {
                    CreateDummyNode(0),
                    CreateDummyNode(1)
                },
                new[]
                {
                    CreateDummyNode(2),
                    CreateDummyNode(3),
                },
                new[]
                {
                    CreateDummyNode(4),
                    CreateDummyNode(5),
                }
            };
            
            var stack1 = new StackState<SymbolicValue<DummyInstruction>>();
            var values1 = new[]
            {
                new SymbolicValue<DummyInstruction>(sources[0][0]),
                new SymbolicValue<DummyInstruction>(sources[1][0]),
                new SymbolicValue<DummyInstruction>(sources[2][0]),
            };
            stack1.Push(values1);
            
            var stack2 = new StackState<SymbolicValue<DummyInstruction>>();
            var values2 = new[]
            {
                new SymbolicValue<DummyInstruction>(sources[0][1]),
                new SymbolicValue<DummyInstruction>(sources[1][1]),
                new SymbolicValue<DummyInstruction>(sources[2][1]),
            };
            stack2.Push(values2);
            
            Assert.True(stack1.MergeWith(stack2));

            int index = sources.Length - 1;
            foreach (var slot in stack1.GetAllStackSlots())
            {
                Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(sources[index]), slot.GetNodes());
                index--;
            }
        }

        [Fact]
        public void MergeStackImbalance()
        {
            var stack1 = new StackState<SymbolicValue<DummyInstruction>>();
            stack1.Push(new[]
            {
                new SymbolicValue<DummyInstruction>(),
                new SymbolicValue<DummyInstruction>(),
            });
            
            var stack2 = new StackState<SymbolicValue<DummyInstruction>>();
            stack2.Push(new[]
            {
                new SymbolicValue<DummyInstruction>(),
            });

            Assert.Throws<StackImbalanceException>(() => stack1.MergeWith(stack2));
        }
    }
}