using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Echo.DataFlow.Emulation;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.DataFlow.Tests.Emulation
{
    public class SymbolicStackStateTest
    {
        private static DataFlowNode<DummyInstruction> CreateDummyNode(long id)
        {
            return new DataFlowNode<DummyInstruction>(DummyInstruction.Op(id, 0, 1));
        }

        [Fact]
        public void MergeSingle()
        {
            var sources = new[]
            {
                CreateDummyNode(0), CreateDummyNode(1),
            };

            var value1 = SymbolicValue<DummyInstruction>.CreateStackValue(sources[0]);
            var stack1 = ImmutableStack.Create(value1);

            var value2 = SymbolicValue<DummyInstruction>.CreateStackValue(sources[1]);
            var stack2 = ImmutableStack.Create(value2);

            var state1 = new SymbolicProgramState<DummyInstruction>(0, stack1);
            var state2 = new SymbolicProgramState<DummyInstruction>(0, stack2);
            
            Assert.True(state1.MergeStates(state2, out var newState));
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(sources), newState.Stack.Peek().GetNodes());
        }

        [Fact]
        public void MergeSingleNoChange()
        {
            var source = CreateDummyNode(0);
            
            var value1 = SymbolicValue<DummyInstruction>.CreateStackValue(source);
            var stack1 = ImmutableStack.Create(value1);
            
            var value2 = SymbolicValue<DummyInstruction>.CreateStackValue(source);
            var stack2 = ImmutableStack.Create(value2);

            var state1 = new SymbolicProgramState<DummyInstruction>(0, stack1);
            var state2 = new SymbolicProgramState<DummyInstruction>(0, stack2);
            
            Assert.False(state1.MergeStates(state2, out var newState));
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(new[] {source}), newState.Stack.Peek().GetNodes());
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
            
            var values1 = new[]
            {
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[0][0]),
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[1][0]),
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[2][0]),
            };

            var stack1 = ImmutableStack.Create(values1);
            
            var values2 = new[]
            {
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[0][1]),
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[1][1]),
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[2][1]),
            };
            
            var stack2 = ImmutableStack.Create(values2);
            
            var state1 = new SymbolicProgramState<DummyInstruction>(0, stack1);
            var state2 = new SymbolicProgramState<DummyInstruction>(0, stack2);

            Assert.True(state1.MergeStates(state2, out var newState));

            int index = sources.Length - 1;
            foreach (var slot in newState.Stack)
            {
                Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(sources[index]), slot.GetNodes());
                index--;
            }
        }

        [Fact]
        public void MergeMultipleSingleChangeShouldOnlyCreateNewInstancesForChangedSlots()
        {
            var nodes = new[]
            {
                CreateDummyNode(0),
                CreateDummyNode(1),
                CreateDummyNode(2),
                CreateDummyNode(3),
            };
            
            var sources = new[]
            {
                new[]
                {
                    nodes[0]
                },
                new[]
                {
                    nodes[1]
                },
                new[]
                {
                    nodes[2],
                    nodes[3],
                }
            };
            
            var values1 = new[]
            {
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[0][0]),
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[1][0]),
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[2][0]),
            };

            var stack1 = ImmutableStack.Create(values1);
            
            var values2 = new[]
            {
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[0][0]),
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[1][0]),
                SymbolicValue<DummyInstruction>.CreateStackValue(sources[2][1]),
            };
            
            var stack2 = ImmutableStack.Create(values2);
            
            var state1 = new SymbolicProgramState<DummyInstruction>(0, stack1);
            var state2 = new SymbolicProgramState<DummyInstruction>(0, stack2);

            Assert.True(state1.MergeStates(state2, out var newState));

            Assert.NotSame(state1.Stack.Peek(), newState.Stack.Peek());
            Assert.Same(state1.Stack.ElementAt(1), newState.Stack.ElementAt(1));
            Assert.Same(state1.Stack.ElementAt(2), newState.Stack.ElementAt(2));
        }

        [Fact]
        public void MergeStackImbalance()
        {
            var stack1 = ImmutableStack.Create(
                new SymbolicValue<DummyInstruction>(),
                new SymbolicValue<DummyInstruction>()
            );

            var stack2 = ImmutableStack.Create(new SymbolicValue<DummyInstruction>());

            var state1 = new SymbolicProgramState<DummyInstruction>(0, stack1);
            var state2 = new SymbolicProgramState<DummyInstruction>(0, stack2);

            Assert.Throws<StackImbalanceException>(() => state1.MergeStates(state2, out _));
        }
    }
}