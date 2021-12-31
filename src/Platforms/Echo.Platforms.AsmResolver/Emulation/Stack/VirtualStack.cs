using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    public class VirtualStack : IMemorySpace, IReadOnlyList<VirtualFrame>
    {
        private readonly List<(long BaseAddress, VirtualFrame Frame)> _frames = new();

        private uint _currentOffset = 0;

        public VirtualStack(int maxSize)
        {
            AddressRange = new AddressRange(0, maxSize);
        }

        public AddressRange AddressRange
        {
            get;
        }
        
        public int Count => _frames.Count;

        public VirtualFrame this[int index] => _frames[index].Frame;
        
        private bool TryFindStackFrame(long address, out (long BaseAddress, VirtualFrame Frame) entry)
        {
            entry = _frames.FirstOrDefault(e =>
            {
                var actualRange = new AddressRange(
                    e.Frame.AddressRange.Start + e.BaseAddress,
                    e.Frame.AddressRange.End + e.BaseAddress);
                return actualRange.Contains(address);
            });

            return entry.Frame is not null;
        }

        public long GetFrameAddress(int index) => _frames[index].BaseAddress;

        public bool IsValidAddress(long address)
        {
            return TryFindStackFrame(address, out var e) && e.Frame.IsValidAddress(e.BaseAddress);
        }

        public void Read(long address, BitVectorSpan buffer)
        {
            if (!TryFindStackFrame(address, out var e))
                throw new AccessViolationException();

            e.Frame.Read(address - e.BaseAddress, buffer);
        }

        public void Write(long address, BitVectorSpan buffer)
        {
            if (!TryFindStackFrame(address, out var e))
                throw new AccessViolationException();

            e.Frame.Write(address - e.BaseAddress, buffer);
        }

        public void Push(VirtualFrame frame)
        {
            uint newOffset = _currentOffset + (uint) frame.Size;
            if (newOffset >= AddressRange.Length)
                throw new StackOverflowException();

            _frames.Add((_currentOffset, frame));
            _currentOffset = newOffset;
        }

        public VirtualFrame Peek()
        {
            if (_frames.Count == 0)
                throw new InvalidOperationException("Stack is empty.");

            var (_, frame) = _frames[_frames.Count - 1];
            return frame;
        }

        public VirtualFrame Pop()
        {
            if (_frames.Count == 0)
                throw new InvalidOperationException("Stack is empty.");

            var (_, frame) = _frames[_frames.Count - 1];
            _frames.RemoveAt(_frames.Count - 1);
            _currentOffset -= (uint) frame.Size;
            return frame;
        }

        public IEnumerator<VirtualFrame> GetEnumerator() => _frames.Select(e => e.Frame).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}