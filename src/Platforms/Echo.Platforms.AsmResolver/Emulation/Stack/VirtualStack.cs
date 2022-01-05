using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Represents a call stack consisting of frames, representing the current state of the program.
    /// </summary>
    public class VirtualStack : IMemorySpace, IReadOnlyList<VirtualFrame>
    {
        private readonly List<(long BaseAddress, VirtualFrame Frame)> _frames = new();

        private uint _currentOffset = 0;

        /// <summary>
        /// Creates a new virtual call stack.
        /// </summary>
        /// <param name="maxSize">The maximum number of bytes the stack can hold.</param>
        public VirtualStack(int maxSize)
        {
            AddressRange = new AddressRange(0, maxSize);
        }

        /// <inheritdoc />
        public int Count => _frames.Count;

        /// <inheritdoc />
        public VirtualFrame this[int index] => _frames[index].Frame;

        /// <inheritdoc />
        public AddressRange AddressRange
        {
            get;
        }

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

        /// <summary>
        /// Gets the address of a single frame in the stack.
        /// </summary>
        /// <param name="index">The index of the stack frame.</param>
        /// <returns>The address.</returns>
        public long GetFrameAddress(int index) => _frames[index].BaseAddress;

        /// <inheritdoc />
        public bool IsValidAddress(long address)
        {
            return TryFindStackFrame(address, out var e) && e.Frame.IsValidAddress(e.BaseAddress);
        }

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            if (!TryFindStackFrame(address, out var e))
                throw new AccessViolationException();

            e.Frame.Read(address - e.BaseAddress, buffer);
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            if (!TryFindStackFrame(address, out var e))
                throw new AccessViolationException();

            e.Frame.Write(address - e.BaseAddress, buffer);
        }

        /// <summary>
        /// Pushes a call frame onto the stack.
        /// </summary>
        /// <param name="frame">The frame to push.</param>
        /// <exception cref="StackOverflowException">Occurs when the stack reached its maximum size.</exception>
        public void Push(VirtualFrame frame)
        {
            uint newOffset = _currentOffset + (uint) frame.Size;
            if (newOffset >= AddressRange.Length)
                throw new StackOverflowException();

            _frames.Add((_currentOffset, frame));
            _currentOffset = newOffset;
        }

        /// <summary>
        /// Gets the top-most frame on the stack.
        /// </summary>
        /// <returns>The frame.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the stack is empty.</exception>
        public VirtualFrame Peek()
        {
            if (_frames.Count == 0)
                throw new InvalidOperationException("Stack is empty.");

            return _frames[_frames.Count - 1].Frame;
        }

        /// <summary>
        /// Pops the top-most frame from the stack.
        /// </summary>
        /// <returns>The popped frame.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the stack is empty.</exception>
        public VirtualFrame Pop()
        {
            if (_frames.Count == 0)
                throw new InvalidOperationException("Stack is empty.");

            var (_, frame) = _frames[_frames.Count - 1];
            _frames.RemoveAt(_frames.Count - 1);
            _currentOffset -= (uint) frame.Size;
            return frame;
        }

        /// <inheritdoc />
        public IEnumerator<VirtualFrame> GetEnumerator() => _frames.Select(e => e.Frame).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}