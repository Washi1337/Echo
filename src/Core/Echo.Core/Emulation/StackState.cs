using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Values;

namespace Echo.Core.Emulation
{
    /// <summary>
    /// Provides a base implementation of a stack state snapshot.
    /// </summary>
    public class StackState<TValue> : IStackState<TValue> 
        where TValue : IValue
    {
        private readonly List<TValue> _stack = new List<TValue>();
        
        /// <inheritdoc />
        public TValue Top => Size == 0 ? default : _stack[Size - 1];

        /// <inheritdoc />
        public int Size => _stack.Count;

        /// <inheritdoc />
        public TValue this[int index] => _stack[_stack.Count - index - 1];

        /// <summary>
        /// Gets an ordered list of items allocated on the stack. The last item in the list represents the top of the stack.
        /// </summary>
        protected List<TValue> Items => _stack;

        /// <inheritdoc />
        public IEnumerable<TValue> GetAllStackSlots()
        {
            return Enumerable.Reverse(_stack);
        }

        /// <inheritdoc />
        public void Push(TValue value)
        {
            _stack.Add(value);
        }

        /// <inheritdoc />
        public void Push(IEnumerable<TValue> values, bool reversed = false)
        {
            if (reversed)
                values = values.Reverse();

            _stack.AddRange(values);
        }

        /// <inheritdoc />
        public TValue Pop()
        {
            AssertCanPop(1);
            
            var top = Top;
            _stack.RemoveAt(Size - 1);
            return top;
        }

        /// <inheritdoc />
        public TValue[] Pop(int count, bool reversed = false)
        {
            AssertCanPop(count);
            
            var values = new TValue[count];
            if (count <= 0)
                return values;
            
            _stack.CopyTo(Size - count, values, 0, count);
            _stack.RemoveRange(Size - count, count);

            if (!reversed)
                Array.Reverse(values);

            return values;
        }

        private void AssertCanPop(int count)
        {
            if (Size < count)
                throw new StackImbalanceException("Insufficient items on the stack.");
        }

        IStackState<TValue> IStackState<TValue>.Copy() => Copy();

        /// <inheritdoc />
        public void Clear() => _stack.Clear();

        /// <summary>
        /// Creates a copy of the stack state. This also copies all values inside the stack.
        /// </summary>
        /// <returns>The copied stack state.</returns>
        public virtual StackState<TValue> Copy()
        {
            var result = new StackState<TValue>();
            foreach (var value in _stack)
                result._stack.Add((TValue) value.Copy());
            return result;
        }

        /// <inheritdoc />
        public override string ToString() => $"Size: {Size}, Top: {Top}";
    }
}