using System.Collections;
using System.Collections.Generic;

namespace Echo.ControlFlow.Serialization.Blocks
{
    /// <summary>
    /// Provides an implementation of a stack of which the elements can be accessed by index.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class IndexableStack<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _items = new List<T>();

        /// <inheritdoc />
        public int Count => _items.Count;

        /// <inheritdoc />
        public T this[int index] => _items[index];
        
        /// <summary>
        /// Returns the top element of the stack.
        /// </summary>
        /// <returns>The top element.</returns>
        public T Peek() => _items[_items.Count - 1];
        
        /// <summary>
        /// Returns the n-th top-most element of the stack.
        /// </summary>
        /// <returns>The element.</returns>
        public T Peek(int index) => _items[_items.Count - 1 - index];

        /// <summary>
        /// Pops a single element from the stack.
        /// </summary>
        /// <returns>The popped element.</returns>
        public T Pop()
        {
            var result = Peek();
            _items.RemoveAt(_items.Count - 1);
            return result;
        }

        /// <summary>
        /// Pushes a single element onto the stack.
        /// </summary>
        /// <param name="value"></param>
        public void Push(T value) => _items.Add(value);

        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}