using System.Collections.Generic;
using System.Linq;
using Echo.Core.Emulation;
using Echo.Core.Tests.Values;
using Xunit;

namespace Echo.Core.Tests.Emulation
{
    public class StackStateTest
    {
        private static IList<DummyValue> CreateDummyValues(int count)
        {
            var result = new DummyValue[count];
            for (int i = 0; i < count; i++)
                result[i] = new DummyValue(i);
            return result;
        }
        
        [Fact]
        public void Empty()
        {
            var stack = new StackState<DummyValue>();

            Assert.Equal(0, stack.Size);
            Assert.Null(stack.Top);
            Assert.Empty(stack.GetAllStackSlots());
        }

        [Fact]
        public void Push()
        {
            var stack = new StackState<DummyValue>();
            var value = new DummyValue();
            
            stack.Push(value);
            Assert.Equal(1, stack.Size);
            Assert.Equal(value, stack.Top);
            Assert.Single(stack.GetAllStackSlots());
        }

        [Fact]
        public void PushMany()
        {
            var stack = new StackState<DummyValue>();
            var values = CreateDummyValues(2);
            
            stack.Push(values);
            Assert.Equal(2, stack.Size);
            Assert.Equal(values.Last(), stack.Top);
            Assert.Equal(values.Reverse(), stack.GetAllStackSlots());   
        }

        [Fact]
        public void PushManyReversed()
        {
            var stack = new StackState<DummyValue>();
            var values = CreateDummyValues(2);
            
            stack.Push(values, reversed: true);
            Assert.Equal(2, stack.Size);
            Assert.Equal(values.First(), stack.Top);
            Assert.Equal(values, stack.GetAllStackSlots());   
        }

        [Fact]
        public void Pop()
        {
            var stack = new StackState<DummyValue>();
            var value = new DummyValue();
            
            stack.Push(value);
            Assert.Equal(value, stack.Pop());
        }

        [Fact]
        public void PopMany()
        {
            var stack = new StackState<DummyValue>();
            var values = CreateDummyValues(2);
            
            stack.Push(values);
            Assert.Equal(values.Reverse(), stack.Pop(2));
        }

        [Fact]
        public void PopManyReversed()
        {
            var stack = new StackState<DummyValue>();
            var values = CreateDummyValues(2);
            
            stack.Push(values);
            Assert.Equal(values, stack.Pop(2, reversed: true));
        }

        [Fact]
        public void Copy()
        {
            var stack = new StackState<DummyValue>();
            stack.Push(CreateDummyValues(2));

            var copy = stack.Copy();
            
            Assert.Equal(stack.Top, copy.Top);
            Assert.Equal(stack.Size, copy.Size);
            Assert.Equal(stack.GetAllStackSlots(), copy.GetAllStackSlots());
        }
    }
}