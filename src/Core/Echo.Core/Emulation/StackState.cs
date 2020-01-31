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
            var top = Top;
            _stack.RemoveAt(Size - 1);
            return top;
        }

        /// <inheritdoc />
        public IList<TValue> Pop(int count, bool reversed = false)
        {
            var values = new TValue[count];
            if (count <= 0)
                return values;
            
            _stack.CopyTo(values, Size - count);
            _stack.RemoveRange(Size - count, count);

            if (!reversed)
                Array.Reverse(values);

            return values;
        }

        IStackState<TValue> IStackState<TValue>.Copy()
        {
            return Copy();
        }

        /// <summary>
        /// Creates a copy of the stack state. This also copies all values inside the stack.
        /// </summary>
        /// <returns>The copied stack state.</returns>
        public virtual StackState<TValue> Copy()
        {
            var result = new StackState<TValue>();
            result._stack.AddRange(_stack);
            return result;
        }
    }
}