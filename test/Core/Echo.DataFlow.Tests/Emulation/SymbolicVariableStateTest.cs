using System.Collections.Generic;
using System.Collections.Immutable;
using Echo.Code;
using Echo.DataFlow.Emulation;
using Echo.Platforms.DummyPlatform.Code;
using Xunit;

namespace Echo.DataFlow.Tests.Emulation
{
    public class SymbolicVariableStateTest
    {
        private static DataFlowNode<DummyInstruction> CreateDummyNode(long id)
        {
            return new DataFlowNode<DummyInstruction>(id, DummyInstruction.Op(id, 0, 1));
        }

        [Fact]
        public void MergeSingleVariable()
        {
            var sources = new[]
            {
                CreateDummyNode(0), CreateDummyNode(1),
            };

            var variable = new DummyVariable("V_1");
            
            var variables1 = ImmutableDictionary<IVariable, SymbolicValue<DummyInstruction>>.Empty;
            variables1 = variables1.SetItem(variable, SymbolicValue<DummyInstruction>.CreateVariableValue(sources[0], variable));
            
            var variables2 = ImmutableDictionary<IVariable, SymbolicValue<DummyInstruction>>.Empty;
            variables2 = variables2.SetItem(variable, SymbolicValue<DummyInstruction>.CreateVariableValue(sources[1], variable));

            var state1 = new SymbolicProgramState<DummyInstruction>(0, variables1);
            var state2 = new SymbolicProgramState<DummyInstruction>(0, variables2);
            
            Assert.True(state1.MergeStates(state2, out var newState));
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(sources), newState.Variables[variable].GetNodes());
        }

        [Fact]
        public void MergeSingleVariableNoChange()
        {
            var source = CreateDummyNode(0);

            var variable = new DummyVariable("V_1");
            
            var variables1 = ImmutableDictionary<IVariable, SymbolicValue<DummyInstruction>>.Empty;
            variables1 = variables1.SetItem(variable, SymbolicValue<DummyInstruction>.CreateVariableValue(source, variable));
            
            var variables2 = ImmutableDictionary<IVariable, SymbolicValue<DummyInstruction>>.Empty;
            variables2 = variables2.SetItem(variable,SymbolicValue<DummyInstruction>.CreateVariableValue(source, variable));

            var state1 = new SymbolicProgramState<DummyInstruction>(0, variables1);
            var state2 = new SymbolicProgramState<DummyInstruction>(0, variables2);
            
            Assert.False(state1.MergeStates(state2, out var newState));
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(new []{source}), newState.Variables[variable].GetNodes());
        }

        [Fact]
        public void MergeMultipleDisjointVariables()
        {
            var sources = new[]
            {
                CreateDummyNode(0), 
                CreateDummyNode(1),
            };

            var variables = new[]
            {
                new DummyVariable("V_1"),
                new DummyVariable("V_2"),
            };
            
            var variables1 = ImmutableDictionary<IVariable, SymbolicValue<DummyInstruction>>.Empty;
            variables1 = variables1.SetItem(variables[0], SymbolicValue<DummyInstruction>.CreateVariableValue(sources[0], variables[0]));
            
            var variables2 = ImmutableDictionary<IVariable, SymbolicValue<DummyInstruction>>.Empty;
            variables2 = variables2.SetItem(variables[1], SymbolicValue<DummyInstruction>.CreateVariableValue(sources[1], variables[1]));

            var state1 = new SymbolicProgramState<DummyInstruction>(0, variables1);
            var state2 = new SymbolicProgramState<DummyInstruction>(0, variables2);
            
            Assert.True(state1.MergeStates(state2, out var newState));
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(new []{sources[0]}), newState.Variables[variables[0]].GetNodes());
            Assert.Equal(new HashSet<DataFlowNode<DummyInstruction>>(new []{sources[1]}), newState.Variables[variables[1]].GetNodes());
        }
    }
}