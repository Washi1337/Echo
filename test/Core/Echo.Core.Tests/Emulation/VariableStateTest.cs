using System.Collections.Generic;
using Echo.Core.Emulation;
using Echo.Platforms.DummyPlatform.Code;
using Echo.Platforms.DummyPlatform.Values;
using Xunit;

namespace Echo.Core.Tests.Emulation
{
    public class VariableStateTest
    {
        private static readonly IList<DummyVariable> _variables = new List<DummyVariable>
        {
            new DummyVariable("var1"),
            new DummyVariable("var2"),
            new DummyVariable("var3"),
            new DummyVariable("var4"),
            new DummyVariable("var5"),
        };
        
        [Fact]
        public void Default()
        {
            var defaultValue = new DummyValue(-1);
            var state = new VariableState<DummyValue>(defaultValue);
            
            foreach (var variable in _variables)
                Assert.Equal(defaultValue, state[variable]);
        }

        [Fact]
        public void Set()
        {
            var defaultValue = new DummyValue(-1);
            var state = new VariableState<DummyValue>(defaultValue);

            var value1 = new DummyValue(1);
            var value2 = new DummyValue(2);
            
            state[_variables[0]] = value1;
            state[_variables[2]] = value2;

            Assert.Equal(value1, state[_variables[0]]);
            Assert.Equal(defaultValue, state[_variables[1]]);
            Assert.Equal(value2, state[_variables[2]]);
            Assert.Equal(defaultValue, state[_variables[3]]);
            Assert.Equal(defaultValue, state[_variables[4]]);
        }
        
        [Fact]
        public void Remove()
        {
            var defaultValue = new DummyValue(-1);
            var state = new VariableState<DummyValue>(defaultValue);

            var value1 = new DummyValue(1);
            var value2 = new DummyValue(2);
            var value3 = new DummyValue(2);
            
            state[_variables[0]] = value1;
            state[_variables[1]] = value2;
            state[_variables[2]] = value3;
            
            bool result = state.Remove(_variables[0]);
            Assert.True(result);
            
            result = state.Remove(_variables[2]);
            Assert.True(result);
            
            result = state.Remove(_variables[4]);
            Assert.False(result);
            
            Assert.Equal(defaultValue, state[_variables[0]]);
            Assert.Equal(value2, state[_variables[1]]);
            Assert.Equal(defaultValue, state[_variables[2]]);
            Assert.Equal(defaultValue, state[_variables[3]]);
            Assert.Equal(defaultValue, state[_variables[4]]);
        }
        
        [Fact]
        public void Clear()
        {
            var defaultValue = new DummyValue(-1);
            var state = new VariableState<DummyValue>(defaultValue);

            var value1 = new DummyValue(1);
            var value2 = new DummyValue(2);
            var value3 = new DummyValue(2);
            
            state[_variables[0]] = value1;
            state[_variables[1]] = value2;
            state[_variables[2]] = value3;

            state.Clear();
            
            Assert.Equal(defaultValue, state[_variables[0]]);
            Assert.Equal(defaultValue, state[_variables[1]]);
            Assert.Equal(defaultValue, state[_variables[2]]);
            Assert.Equal(defaultValue, state[_variables[3]]);
            Assert.Equal(defaultValue, state[_variables[4]]);
        }
    }
}