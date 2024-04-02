using System;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Runtime;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Stack
{
    public class VirtualStackTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly ValueFactory _factory;

        public VirtualStackTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            _factory = new ValueFactory(new RuntimeTypeManager(fixture.CurrentTestModule, false));
        }

        [Fact]
        public void EmptyStack()
        {
            var stack = new CallStack(0x1000, _factory);
            Assert.True(stack.Peek().IsRoot);
            Assert.Throws<InvalidOperationException>(() => stack.Pop());
        }

        [Fact]
        public void PushPeek()
        {
            var stack = new CallStack(0x1000, _factory);

            var frame = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            stack.Push(frame);
            
            Assert.Equal(frame, stack.Peek());
        }

        [Fact]
        public void PushMany()
        {
            var stack = new CallStack(0x1000, _factory);

            var frame1 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            var frame2 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            var frame3 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            stack.Push(frame1);
            stack.Push(frame2);
            stack.Push(frame3);
            
            Assert.Equal(frame3, stack.Peek());
            Assert.Equal(frame3, stack.Pop());
            Assert.Equal(frame2, stack.Peek());
            Assert.Equal(frame2, stack.Pop());
            Assert.Equal(frame1, stack.Peek());
            Assert.Equal(frame1, stack.Pop());
        }

        [Fact]
        public void PushPopPushShouldGetSameAddressAsPreviousStackFrame()
        {
            var stack = new CallStack(0x1000, _factory);
            
            var frame1 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            var frame2 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            stack.Push(frame1);
            stack.Push(frame2);

            long address = stack.GetFrameAddress(1);
            Assert.NotEqual(0, address);
            stack.Pop();
            
            var frame3 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            stack.Push(frame3);
            
            Assert.Equal(address, stack.GetFrameAddress(1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void ReadIntoStackFrame(long baseAddress)
        {
            var stack = new CallStack(0x1000, _factory);
            stack.Rebase(baseAddress);
            
            var frame1 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            var frame2 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            stack.Push(frame1);
            stack.Push(frame2);
            
            frame2.WriteLocal(1, new BitVector(BitConverter.GetBytes(0x1337)).AsSpan());

            long address = frame2.GetLocalAddress(1);
            var buffer = new BitVector(32, false).AsSpan();
            stack.Read(address, buffer);
            
            Assert.Equal(0x1337, BitConverter.ToInt32(buffer.Bits));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void WriteIntoStackFrame(long baseAddress)
        {
            var stack = new CallStack(0x1000, _factory);
            stack.Rebase(baseAddress);
            
            var frame1 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            var frame2 = new CallFrame(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)), _factory);
            stack.Push(frame1);
            stack.Push(frame2);

            var readBuffer = new BitVector(32, false).AsSpan();
            frame2.ReadLocal(1, readBuffer);
            Assert.Equal(0, BitConverter.ToInt32(readBuffer.Bits));

            long address = frame2.GetLocalAddress(1);            
            stack.Write(address, new BitVector(BitConverter.GetBytes(0x1337)).AsSpan());
            
            frame2.ReadLocal(1, readBuffer);
            Assert.Equal(0x1337, BitConverter.ToInt32(readBuffer.Bits));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void AllocateShouldUpdateStackPointer(long baseAddress)
        {
            var stack = new CallStack(0x1000, _factory);
            stack.Rebase(baseAddress);
            long originalStackPointer = stack.StackPointer;
            
            // Push one frame.
            var frame = stack.Push(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)));
            int frameSize = frame.Size;
            Assert.Equal(originalStackPointer + frameSize, stack.StackPointer);
            
            // Allocate extra memory in the frame.
            frame.Allocate(100);
            Assert.Equal(originalStackPointer + frameSize + 100, stack.StackPointer);

            // Remove frame.
            stack.Pop();
            Assert.Equal(originalStackPointer, stack.StackPointer);
        }

        [Fact]
        public void AllocateShouldOnlyBeAllowedForTopFrame()
        {
            var stack = new CallStack(0x1000, _factory);
            
            var frame1 = stack.Push(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)));
            frame1.Allocate(10);
            
            var frame2 = stack.Push(_fixture.GetTestMethod(nameof(TestClass.MultipleLocalsMultipleArguments)));
            Assert.ThrowsAny<InvalidOperationException>(() => frame1.Allocate(10));
            frame2.Allocate(10);

            stack.Pop();
            
            frame1.Allocate(10);
        }
        
    }
}