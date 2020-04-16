using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Echo.Concrete.Values
{
    public struct MagicBox<T> where T : unmanaged
    {
        public MagicBox(T value)
        {
            _value = value;
        }

        private T _value;

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

        public unsafe Span<byte> GetBitsUnsafe()
        {
            return new Span<byte>(Unsafe.AsPointer(ref this), sizeof(T));
        }

        public unsafe void GetBits(Span<byte> buffer)
        {
            fixed (MagicBox<T>* ptr = &this)
            {
                new Span<byte>(ptr, Unsafe.SizeOf<T>()).CopyTo(buffer);
            }
        }

        public void Not()
        {
            var span = GetBitsUnsafe();
            for (var i = 0; i < span.Length; i++)
            {
                span[i] = (byte)~span[i];
            }
        }

        public void And(MagicBox<T> other)
        {
            var @this = GetBitsUnsafe();
            var that = other.GetBitsUnsafe();

            for (var i = 0; i < @this.Length; i++)
            {
                @this[i] &= that[i];
            }
        }

        public void Or(MagicBox<T> other)
        {
            var @this = GetBitsUnsafe();
            var that = other.GetBitsUnsafe();

            for (var i = 0; i < @this.Length; i++)
            {
                @this[i] |= that[i];
            }
        }

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
        public override bool Equals(object? obj)
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
            var hash = 0;
            foreach (var bit in GetBitsUnsafe())
            {
                hash += bit * 397;
            }

            return 0x5321 ^ (hash * 679);
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

        public static explicit operator T(MagicBox<T> magicBox)
        {
            return magicBox._value;
        }

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