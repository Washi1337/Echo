using System;
using System.Diagnostics.CodeAnalysis;
using AsmResolver.IO;
using Echo.Concrete;
using Echo.Concrete.Memory;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class VirtualDataSource : IDataSource
    {
        private readonly VirtualMemory _memory;
        private BitVector? _buffer;

        public VirtualDataSource(VirtualMemory memory, long baseAddress, long size)
        {
            _memory = memory;
            BaseAddress = (ulong) baseAddress;
            Length = (ulong) size;
        }

        public byte this[ulong address]
        {
            get
            {
                EnsureCapacity(1);
                _memory.Read((long) address, _buffer.AsSpan(0, 8));
                return _buffer.Bits[0];
            }
        }

        public ulong BaseAddress
        {
            get;
        }

        public ulong Length
        {
            get;
        }

        [MemberNotNull(nameof(_buffer))]
        private void EnsureCapacity(int count)
        {
            if (_buffer is null || _buffer.Count / 8 < count)
                _buffer = new BitVector(count * 8, false);
        }

        public bool IsValidAddress(ulong address) => address >= BaseAddress && address < BaseAddress + Length;

        public int ReadBytes(ulong address, byte[] buffer, int index, int count)
        {
            EnsureCapacity(count);

            _memory.Read((long) address, _buffer.AsSpan(0, count*8));
            Buffer.BlockCopy(_buffer.Bits, 0, buffer, index, count);
            return count;
        }
    }
}