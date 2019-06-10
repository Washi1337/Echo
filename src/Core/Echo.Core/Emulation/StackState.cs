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
        private readonly Stack<TValue> _stack = new Stack<TValue>();

        /// <inheritdoc />
        public TValue Top => _stack.Count == 0 ? default : _stack.Peek();

        /// <inheritdoc />
        public int Size => _stack.Count;

        /// <inheritdoc />
        public IEnumerable<TValue> GetAllStackSlots()
        {
            return _stack.AsEnumerable();
        }

        /// <inheritdoc />
        public void Push(TValue value)
        {
            _stack.Push(value);
        }

        /// <inheritdoc />
        public void Push(IEnumerable<TValue> values, bool reversed = false)
        {
            if (reversed)
                values = values.Reverse();
            
            foreach (var value in values)
                _stack.Push(value);
        }

        /// <inheritdoc />
        public TValue Pop()
        {
            return _stack.Pop();
        }

        /// <inheritdoc />
        public IList<TValue> Pop(int count, bool reversed = false)
        {
            var values = new TValue[count];

            for (int i = 0; i < count; i++)
            {
                int index = reversed ? count - i - 1 : i;
                values[index] = _stack.Pop();
            }

            return values;
        }

        IStackState<TValue> IStackState<TValue>.Copy()
        {
            return Copy();
        }

        /// <summary>
        /// Creates a copy of the stack state.
        /// </summary>
        /// <returns>The copied stack state.</returns>
        public StackState<TValue> Copy()
        {
            var result = new StackState<TValue>();
            foreach (var value in _stack.Reverse())
                result.Push(value);
            return result;
        }
    }
}