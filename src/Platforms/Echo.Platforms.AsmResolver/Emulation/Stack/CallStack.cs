using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Core.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Represents a call stack consisting of frames, representing the current state of the program.
    /// </summary>
    public class CallStack : IMemorySpace, IReadOnlyList<CallFrame>
    {
        private readonly List<CallFrame> _frames = new();

        private readonly ValueFactory _factory;

        /// <summary>
        /// Creates a new virtual call stack.
        /// </summary>
        /// <param name="maxSize">The maximum number of bytes the stack can hold.</param>
        /// <param name="factory">The service responsible for managing types.</param>
        public CallStack(int maxSize, ValueFactory factory)
        {
            _factory = factory;
            AddressRange = new AddressRange(0, maxSize);

            var rootMethod = new MethodDefinition("<<root>>", 0, 
                MethodSignature.CreateStatic(factory.ContextModule.CorLibTypeFactory.Void));
            Push(new CallFrame(rootMethod, factory, true));
        }

        /// <inheritdoc />
        public int Count => _frames.Count;

        /// <inheritdoc />
        public CallFrame this[int index] => _frames[index];

        /// <inheritdoc />
        public AddressRange AddressRange
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current value of the stack pointer.
        /// </summary>
        public long StackPointer => _frames.Count > 0 
            ? _frames[_frames.Count - 1].AddressRange.End 
            : AddressRange.Start;

        private bool TryFindStackFrame(long address, [NotNullWhen(true)] out CallFrame? entry)
        {
            entry = _frames.FirstOrDefault(e => e.AddressRange.Contains(address));
            return entry is not null;
        }

        /// <summary>
        /// Gets the address of a single frame in the stack.
        /// </summary>
        /// <param name="index">The index of the stack frame.</param>
        /// <returns>The address.</returns>
        public long GetFrameAddress(int index) => _frames[index].AddressRange.Start;

        /// <inheritdoc />
        public bool IsValidAddress(long address)
        {
            return TryFindStackFrame(address, out var e) && e.IsValidAddress(address);
        }

        /// <inheritdoc />
        public void Rebase(long baseAddress)
        {
            long oldBase = AddressRange.Start;
            AddressRange = new AddressRange(baseAddress, baseAddress + AddressRange.Length);
            foreach (var entry in _frames)
                entry.Rebase(entry.AddressRange.Start - oldBase + AddressRange.Start);
        }

        /// <inheritdoc />
        public void Read(long address, BitVectorSpan buffer)
        {
            if (!TryFindStackFrame(address, out var e))
                throw new AccessViolationException();

            e.Read(address, buffer);
        }

        /// <inheritdoc />
        public void Write(long address, BitVectorSpan buffer)
        {
            if (!TryFindStackFrame(address, out var e))
                throw new AccessViolationException();

            e.Write(address, buffer);
        }

        /// <inheritdoc />
        public void Write(long address, ReadOnlySpan<byte> buffer)
        {
            if (!TryFindStackFrame(address, out var e))
                throw new AccessViolationException();

            e.Write(address, buffer);
        }

        /// <summary>
        /// Creates a new call stack frame for the provided method and pushes it onto the top of the call stack.
        /// </summary>
        /// <param name="method">The method to create a frame for.</param>
        /// <returns>The created call stack frame.</returns>
        public CallFrame Push(IMethodDescriptor method)
        {
            var frame = new CallFrame(method, _factory);
            Push(frame);
            return frame;
        }

        /// <summary>
        /// Pushes a call frame onto the stack.
        /// </summary>
        /// <param name="frame">The frame to push.</param>
        /// <exception cref="StackOverflowException">Occurs when the stack reached its maximum size.</exception>
        public void Push(CallFrame frame)
        {
            long newStackPointer = StackPointer + (uint) frame.Size;
            if (newStackPointer >= AddressRange.End)
                throw new StackOverflowException();

            if (_frames.Count > 0)
                Peek().CanAllocateMemory = false;
            
            frame.Rebase(StackPointer);
            _frames.Add(frame);

            frame.CanAllocateMemory = true;
        }

        /// <summary>
        /// Gets the top-most frame on the stack.
        /// </summary>
        /// <returns>The frame.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the stack is empty.</exception>
        public CallFrame Peek()
        {
            if (_frames.Count == 0)
                throw new InvalidOperationException("Stack is empty.");

            return _frames[_frames.Count - 1];
        }

        /// <summary>
        /// Pops the top-most frame from the stack.
        /// </summary>
        /// <returns>The popped frame.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the stack is empty.</exception>
        public CallFrame Pop()
        {
            if (_frames.Count == 1)
                throw new InvalidOperationException("Cannot pop the root stack-frame.");

            var frame = _frames[_frames.Count - 1];
            _frames.RemoveAt(_frames.Count - 1);

            frame.CanAllocateMemory = false;
            Peek().CanAllocateMemory = true;

            return frame;
        }

        /// <inheritdoc />
        public IEnumerator<CallFrame> GetEnumerator() => _frames.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}