using System;
using Echo.Core;

namespace Echo.Concrete
{
    public readonly ref partial struct BitVectorSpan
    {
        /// <summary>
        /// Interprets the bit vector as an integer and adds a second integer to it. 
        /// </summary>
        /// <param name="other">The integer to add.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        /// <returns>The value of the carry bit after the addition completed.</returns>
        public Trilean IntegerAdd(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            var carry = Trilean.False;

            for (int i = 0; i < Count; i++)
            {
                var a = this[i];
                var b = other[i];

                // Implement full-adder logic.
                var s = a ^ b ^ carry;
                var c = (carry & (a ^ b)) | (a & b);

                this[i] = s;
                carry = c;
            }

            return carry;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and increments it by one. 
        /// </summary>
        /// <returns>The value of the carry bit after the increment operation completed.</returns>
        public Trilean IntegerIncrement()
        {
            // Optimized version of full-adder that does not require allocation of another vector, and short circuits
            // after carry does not have any effect any more. 
            
            var carry = Trilean.True;

            for (int i = 0; i < Count && carry != Trilean.False; i++)
            {
                var a = this[i];

                // Implement reduced adder logic.
                var s = a ^ carry;
                var c = carry & a;

                this[i] = s;
                carry = c;
            }

            return carry;
        }

        /// <summary>
        /// Interprets the bit vector as an integer and negates it according to the two's complement semantics.
        /// </summary>
        public void IntegerNegate()
        {
            Not();
            IntegerIncrement();
        }

        /// <summary>
        /// Interprets the bit vector as an integer and subtracts a second integer from it.
        /// </summary>
        /// <param name="other">The integer to subtract.</param>
        /// <exception cref="ArgumentException">Occurs when the sizes of the integers do not match in bit length.</exception>
        /// <returns>The value of the borrow bit after the subtraction completed.</returns>
        public Trilean IntegerSubtract(BitVectorSpan other)
        {
            AssertSameBitSize(other);

            var borrow = Trilean.False;
            
            for (int i = 0; i < Count; i++)
            {
                var a = this[i];
                var b = other[i];

                // Implement full-subtractor logic.
                var d = a ^ b ^ borrow;
                var bOut = (!a & borrow) | (!a & b) | (b & borrow);

                this[i] = d;
                borrow = bOut;
            }

            return borrow;
        }
        
        /// <summary>
        /// Interprets the bit vector as an integer and decrements it by one. 
        /// </summary>
        /// <returns>The value of the carry bit after the decrement operation completed.</returns>
        public Trilean IntegerDecrement()
        {
            // Optimized version of full-subtractor that does not require allocation of another vector, and short
            // circuits after borrow does not have any effect any more. 
            
            var borrow = Trilean.True;
            
            for (int i = 0; i < Count && borrow != Trilean.False; i++)
            {
                var a = this[i];

                // Implement reduced subtractor logic.
                var d = a ^ borrow;
                var bOut = !a & borrow;

                this[i] = d;
                borrow = bOut;
            }

            return borrow;
        }
    }
}