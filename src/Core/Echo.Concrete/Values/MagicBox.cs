using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Echo.Concrete.Values
{
    /// <summary>
    /// Performs various dark magic to achieve raw bit manipulation
    /// </summary>
    public readonly ref struct MagicBox
    {
        /// <summary>
        /// Creates a new <see cref="MagicBox"/> with the given <paramref name="span"/>
        /// </summary>
        /// <param name="span">The value to initialize the <see cref="MagicBox"/> with</param>
        public MagicBox(Span<byte> span)
        {
            _span = span;
        }

        private readonly Span<byte> _span;

        /// <summary>
        /// Gets the bit at the <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index to get the bit at</param>
        public bool this[int index]
        {
            get
            {
                ValidateIndex(index);

                return ((_span[Math.DivRem(index, 8, out var q)] >> q) & 1) != 0;
            }
            set
            {
                ValidateIndex(index);
                
                if (value)
                {
                    _span[Math.DivRem(index, 8, out var q)] |= (byte) (1 << q);
                }
                else
                {
                    _span[Math.DivRem(index, 8, out var q)] &= (byte) ~(1 << q);
                }
            }
        }

        /// <summary>
        /// Performs a bitwise NOT operation on <see cref="MagicBox"/>
        /// </summary>
        public void Not()
        {
            for (var i = 0; i < _span.Length; i++)
            {
                _span[i] = (byte) ~_span[i];
            }
        }

        /// <summary>
        /// Performs a bitwise AND operation between two <see cref="MagicBox"/>'s
        /// </summary>
        /// <param name="other">The right side of the expression</param>
        public void And(MagicBox other)
        {
            for (var i = 0; i < other._span.Length; i++)
            {
                _span[i] &= other._span[i];
            }
        }

        /// <summary>
        /// Performs a bitwise OR operation between two <see cref="MagicBox"/>'s
        /// </summary>
        /// <param name="other">The right side of the expression</param>
        public void Or(MagicBox other)
        {
            for (var i = 0; i < other._span.Length; i++)
            {
                _span[i] |= other._span[i];
            }
        }

        /// <summary>
        /// Performs a bitwise XOR operation between two <see cref="MagicBox"/>'s
        /// </summary>
        /// <param name="other">The right side of the expression</param>
        public void Xor(MagicBox other)
        {
            for (var i = 0; i < other._span.Length; i++)
            {
                _span[i] ^= other._span[i];
            }
        }

        /// <summary>
        /// Compares two <see cref="MagicBox"/>'s
        /// </summary>
        /// <remarks>
        /// This overload exists to avoid a nasty boxing allocation
        /// </remarks>
        /// <param name="other">The <see cref="MagicBox"/> to compare to</param>
        /// <returns>Whether the two <see cref="MagicBox"/>'s are equal</returns>
        public bool Equals(MagicBox other)
        {
            return _span.SequenceEqual(other._span);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is MagicBox other)
            {
                return Equals(other);
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var bit in _span)
                {
                    hash += bit * 397;
                }

                return 0x5321 ^ (hash * 679);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var size = _span.Length * 8;
            var sb = new StringBuilder(size);

            for (var i = 0; i < size; i++)
            {
                sb.Append(this[i] ? '1' : '0');
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ValidateIndex(int index)
        {
            var max = _span.Length * 8;
            if (index < 0 || index > max)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be 0 < x < {max}");
            }
        }
    }
}