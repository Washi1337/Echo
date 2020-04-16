using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Echo.Concrete.Values
{
    /// <summary>
    /// Performs various dark magic to achieve raw bit manipulation
    /// </summary>
    /// <typeparam name="T">Determines the size of the <see cref="MagicBox{T}"/></typeparam>
    public struct MagicBox<T> where T : unmanaged
    {
        /// <summary>
        /// Creates a new <see cref="MagicBox{T}"/> with the given <paramref name="value"/>
        /// </summary>
        /// <param name="value">The value to initialize the <see cref="MagicBox{T}"/> with</param>
        public MagicBox(T value)
        {
            _value = value;
        }

        private T _value;

        /// <summary>
        /// Gets the bit at the <paramref name="index"/>
        /// </summary>
        /// <param name="index">The index to get the bit at</param>
        public bool this[int index]
        {
            get
            {
                ValidateIndex(index);

                ref var ptr = ref Unsafe.As<MagicBox<T>, byte>(ref this);
                return ((Unsafe.Add(ref ptr, Math.DivRem(index, 8, out var q)) >> q) & 1) != 0;
            }
            set
            {
                ValidateIndex(index);
                
                ref var ptr = ref Unsafe.As<MagicBox<T>, byte>(ref this);
                ref var @ref = ref Unsafe.Add(ref ptr, Math.DivRem(index, 8, out var q));
                if (value)
                {
                    Unsafe.WriteUnaligned(ref @ref, @ref | (1 << q));
                }
                else
                {
                    Unsafe.WriteUnaligned(ref @ref, @ref & ~(1 << q));
                }
            }
        }

        /// <summary>
        /// Gets the raw bits of <see cref="MagicBox{T}"/>
        /// </summary>
        /// <param name="buffer">The buffer to copy the raw bits into</param>
        public unsafe void GetBits(Span<byte> buffer)
        {
            ref var source = ref Unsafe.As<T, byte>(ref _value);
            ref var destination = ref buffer[0];
            Unsafe.CopyBlockUnaligned(ref destination, ref source, (uint) sizeof(T));
        }

        /// <summary>
        /// Performs a bitwise NOT operation on <see cref="MagicBox{T}"/>
        /// </summary>
        public void Not()
        {
            var span = GetBitsUnsafe();
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = (byte)~span[i];
            }
        }

        /// <summary>
        /// Performs a bitwise AND operation between two <see cref="MagicBox{T}"/>'s
        /// </summary>
        /// <param name="other">The right side of the expression</param>
        public void And(MagicBox<T> other)
        {
            var @this = GetBitsUnsafe();
            var that = other.GetBitsUnsafe();

            for (var i = 0; i < @this.Length; i++)
            {
                @this[i] &= that[i];
            }
        }

        /// <summary>
        /// Performs a bitwise OR operation between two <see cref="MagicBox{T}"/>'s
        /// </summary>
        /// <param name="other">The right side of the expression</param>
        public void Or(MagicBox<T> other)
        {
            var @this = GetBitsUnsafe();
            var that = other.GetBitsUnsafe();

            for (var i = 0; i < @this.Length; i++)
            {
                @this[i] |= that[i];
            }
        }

        /// <summary>
        /// Performs a bitwise XOR operation between two <see cref="MagicBox{T}"/>'s
        /// </summary>
        /// <param name="other">The right side of the expression</param>
        public void Xor(MagicBox<T> other)
        {
            var @this = GetBitsUnsafe();
            var that = other.GetBitsUnsafe();

            for (var i = 0; i < @this.Length; i++)
            {
                @this[i] ^= that[i];
            }
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is MagicBox<T> other)
            {
                return GetBitsUnsafe().SequenceEqual(other.GetBitsUnsafe());
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 0;
                foreach (var bit in GetBitsUnsafe())
                {
                    hash += bit * 397;
                }

                return 0x5321 ^ (hash * 679);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var size = Unsafe.SizeOf<T>() * 8;
            var sb = new StringBuilder(size);

            for (var i = 0; i < size; i++)
            {
                sb.Append(this[i] ? '1' : '0');
            }

            return sb.ToString();
        }

        private unsafe Span<byte> GetBitsUnsafe()
        {
            return new Span<byte>(Unsafe.AsPointer(ref this), sizeof(T));
        }
        
        /// <summary>
        /// Gets the raw <typeparamref name="T"/> value from a <see cref="MagicBox{T}"/>
        /// </summary>
        /// <param name="magicBox">The <see cref="MagicBox{T}"/> to get the raw value of</param>
        /// <returns>The raw <typeparamref name="T"/> <paramref name="magicBox"/> contained</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator T(MagicBox<T> magicBox)
        {
            return magicBox._value;
        }

        /// <summary>
        /// Creates a new <see cref="MagicBox{T}"/> with the provided value
        /// </summary>
        /// <param name="value">The value to initialize with</param>
        /// <returns>A new <see cref="MagicBox{T}"/> instance initialized with <paramref name="value"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator MagicBox<T>(T value)
        {
            return new MagicBox<T>(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ValidateIndex(int index)
        {
            var max = Unsafe.SizeOf<T>() * 8 - 1;
            if (index < 0 || index > max)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index must be 0 < x < {max}");
            }
        }
    }
}