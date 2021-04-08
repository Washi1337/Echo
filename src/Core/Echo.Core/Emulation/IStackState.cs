using System.Collections.Generic;
using Echo.Core.Values;

namespace Echo.Core.Emulation
{
    /// <summary>
    /// Represents a snapshot of the stack at a particular point in time during an execution of a program.
    /// </summary>
    /// <typeparam name="TValue">The type to use to model the slots in the stack.</typeparam>
    public interface IStackState<TValue>
        where TValue : IValue
    {
        /// <summary>
        /// Gets the top value of the stack.
        /// </summary>
        TValue Top
        {
            get;
        }

        /// <summary>
        /// Gets the number of elements on the stack.
        /// </summary>
        int Size
        {
            get;
        }

        /// <summary>
        /// Gets the value at the provided index.
        /// </summary>
        /// <param name="index">The index.</param>
        TValue this[int index]
        {
            get;
        }

        /// <summary>
        /// Gets an ordered collection of all slots that are in use. 
        /// </summary>
        /// <returns>The collection of slots, ordered from top to bottom.</returns>
        IEnumerable<TValue> GetAllStackSlots();
        
        /// <summary>
        /// Pushes a single value onto the stack.
        /// </summary>
        /// <param name="value">The value to push.</param>
        void Push(TValue value);
        
        /// <summary>
        /// Pushes a collection of values onto the stack.
        /// </summary>
        /// <param name="values">The values to push.</param>
        /// <param name="reversed">True indicates whether the collection of values should be pushed in reversed order.</param>
        void Push(IEnumerable<TValue> values, bool reversed = false);
        
        /// <summary>
        /// Pops a single value from the top of the stack.
        /// </summary>
        /// <returns>The value that was popped from the stack.</returns>
        TValue Pop();

        /// <summary>
        /// Pops a collection of values from the stack.
        /// </summary>
        /// <param name="count">The number of values to pop from the stack.</param>
        /// <param name="reversed">True indicates whether the collection of values should be returned in reversed order.</param>
        /// <returns>The popped values.</returns>
        TValue[] Pop(int count, bool reversed = false);
        
        /// <summary>
        /// Creates a copy of the stack state. This also copies all values inside the stack.
        /// </summary>
        /// <returns>The copied stack state.</returns>
        IStackState<TValue> Copy();

        /// <summary>
        /// Removes all slots from the stack.
        /// </summary>
        void Clear();
    }
}