using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Values;

namespace Echo.Concrete.Values.ReferenceType
{
    /// <summary>
    /// Represents a fixed-size collection of values that is passed on by reference.
    /// </summary>
    public class ArrayValue : IConcreteValue, IEnumerable<IConcreteValue>
    {
        private readonly IConcreteValue[] _values;

        /// <summary>
        /// Creates a new empty array.
        /// </summary>
        public ArrayValue()
        {
            _values = new IConcreteValue[0];
        }

        /// <summary>
        /// Creates a new array filled with copies of the provided default value.
        /// </summary>
        /// <param name="length">The length of the array.</param>
        /// <param name="defaultValue">The value to fill in the array with.</param>
        public ArrayValue(int length, IConcreteValue defaultValue)
        {
            _values = new IConcreteValue[length];
            for (int i = 0; i < length; i++)
                _values[i] = (IConcreteValue) defaultValue.Copy();
        }

        /// <summary>
        /// Wraps a collection of elements into an array value.
        /// </summary>
        public ArrayValue(IEnumerable<IConcreteValue> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            
            _values = values as IConcreteValue[] ?? values.ToArray();
        }

        /// <summary>
        /// Gets the length of the array.
        /// </summary>
        public int Length => _values.Length;

        /// <summary>
        /// Gets or sets the value of an element at the provided index.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <exception cref="ArgumentNullException">Occurs when the set value is <c>null</c>.</exception>
        public IConcreteValue this[int index]
        {
            get => _values[index];
            set => _values[index] = value ?? throw new ArgumentNullException();
        }

        /// <inheritdoc />
        public bool IsKnown => true;

        /// <inheritdoc />
        int IValue.Size => 4; // TODO: adjust for 32-bit and 64-bit ptrs.

        /// <inheritdoc />
        public bool IsValueType => false;
        
        bool? IConcreteValue.IsZero => false;

        bool? IConcreteValue.IsNonZero => true;

        bool? IConcreteValue.IsPositive => true;

        bool? IConcreteValue.IsNegative => false;

        /// <inheritdoc />
        public IValue Copy() => new ArrayValue(_values);

        /// <inheritdoc />
        public override string ToString() => $"Array[{Length}]";

        /// <summary>
        /// Returns an enumerator that enumerates through all elements in the array.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public ArrayValueEnumerator GetEnumerator() => new ArrayValueEnumerator(this);

        IEnumerator<IConcreteValue> IEnumerable<IConcreteValue>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Provides an implementation for an enumerator of the <see cref="ArrayValue"/> class.
        /// </summary>
        public struct ArrayValueEnumerator : IEnumerator<IConcreteValue>
        {
            private readonly ArrayValue _array;
            private int _currentIndex;

            /// <summary>
            /// Creates a new instance of the <see cref="ArrayValueEnumerator"/> structure.
            /// </summary>
            /// <param name="array">The array to enumerate.</param>
            public ArrayValueEnumerator(ArrayValue array)
            {
                _array = array;
                _currentIndex = -1;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < _array.Length;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _currentIndex = -1;
            }

            /// <inheritdoc />
            public IConcreteValue Current => _currentIndex >= 0 && _currentIndex < _array.Length
                    ? _array[_currentIndex]
                    : null;

            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose()
            {
            }
        }
    }
}