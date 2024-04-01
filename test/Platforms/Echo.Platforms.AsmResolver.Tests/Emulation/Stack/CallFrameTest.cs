using System;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Stack
{
    public class CallFrameTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly ValueFactory _factory;

        public CallFrameTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            _factory = new ValueFactory(fixture.CurrentTestModule, false);
        }

        [Theory]
        [InlineData(nameof(TestClass.NoLocalsNoArguments), 8)]
        [InlineData(nameof(TestClass.SingleArgument), 8 + 1 * 8)]
        [InlineData(nameof(TestClass.MultipleArguments), 8 + 3 * 8)]
        [InlineData(nameof(TestClass.SingleLocal), 1 * 8 + 8)]
        [InlineData(nameof(TestClass.MultipleLocals), 3 * 8 + 8)]
        [InlineData(nameof(TestClass.MultipleLocalsMultipleArguments), 3 * 8 + 8 + 3 * 8)]
        [InlineData(nameof(TestClass.ValueTypeArgument), 8 + 3 * 8)]
        [InlineData(nameof(TestClass.RefTypeArgument), 8 + 2 * 8)]
        public void FrameShouldAllocateSufficientSize(string name, int expected)
        {
            var method = _fixture.GetTestMethod(name);
            var frame = new CallFrame(method, _factory);
            Assert.Equal(expected, frame.AddressRange.Length);
        }

        [Theory]
        [InlineData(nameof(TestClass.MultipleLocals), 0, "00000000000000000000000000000000")]
        [InlineData(nameof(TestClass.MultipleLocals), 0x7fff_0000, "00000000000000000000000000000000")]
        [InlineData(nameof(TestClass.MultipleLocalsNoInit), 0, "????????????????????????????????")]
        [InlineData(nameof(TestClass.MultipleLocalsNoInit), 0x7fff_0000, "????????????????????????????????")]
        public void ReadLocalTest(string name, long baseAddress, string expected)
        {
            var method = _fixture.GetTestMethod(name);
            var frame = new CallFrame(method, _factory);
            frame.Rebase(baseAddress);

            var buffer = new BitVector(32, false).AsSpan();
            frame.ReadLocal(0, buffer);
            Assert.Equal(expected, buffer.ToBitString());
        } 

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void WriteLocalTest(long baseAddress)
        {
            var method = _fixture.GetTestMethod(nameof(TestClass.MultipleLocals));
            var frame = new CallFrame(method, _factory);
            frame.Rebase(baseAddress);

            var readBuffer = new BitVector(32, false).AsSpan();
            frame.ReadLocal(0, readBuffer);
            Assert.Equal("00000000000000000000000000000000", readBuffer.ToBitString());

            const uint number = 0b10101010_11001100_11110000_11111111;
            
            var writeBuffer = new BitVector(BitConverter.GetBytes(number)).AsSpan();
            frame.WriteLocal(0, writeBuffer);
            
            frame.ReadLocal(0, readBuffer);
            Assert.Equal("10101010110011001111000011111111", readBuffer.ToBitString());
        } 

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void ReadArgumentTest(long baseAddress)
        {
            var method = _fixture.GetTestMethod(nameof(TestClass.MultipleArguments));
            var frame = new CallFrame(method, _factory);
            frame.Rebase(baseAddress);

            var buffer = new BitVector(32, false).AsSpan();
            frame.ReadArgument(0, buffer);
            Assert.Equal("????????????????????????????????", buffer.ToBitString());
        } 

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void WriteArgumentTest(long baseAddress)
        {
            var method = _fixture.GetTestMethod(nameof(TestClass.MultipleArguments));
            var frame = new CallFrame(method, _factory);
            frame.Rebase(baseAddress);

            var readBuffer = new BitVector(32, false).AsSpan();
            frame.ReadArgument(0, readBuffer);
            Assert.Equal("????????????????????????????????", readBuffer.ToBitString());

            const uint number = 0b10101010_11001100_11110000_11111111;
            
            var writeBuffer = new BitVector(BitConverter.GetBytes(number)).AsSpan();
            frame.WriteArgument(0, writeBuffer);
            
            frame.ReadArgument(0, readBuffer);
            Assert.Equal("10101010110011001111000011111111", readBuffer.ToBitString());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void AllocateShouldResizeLocalStorage(long baseAddress)
        {
            var method = _fixture.GetTestMethod(nameof(TestClass.MultipleArguments));
            var stack = new CallStack(1000, _factory);
            stack.Rebase(baseAddress);
            var frame = stack.Push(method);

            long originalSize = frame.Size;
            long pointer = frame.AddressRange.End;
            
            long address = frame.Allocate(100);

            Assert.Equal(pointer , address);
            Assert.Equal(originalSize + 100, frame.Size);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void AllocateShouldRetainArguments(long baseAddress)
        {
            var method = _fixture.GetTestMethod(nameof(TestClass.MultipleArguments));
            var stack = new CallStack(1000, _factory);
            stack.Rebase(baseAddress);
            
            var frame = stack.Push(method);
            frame.WriteArgument(0, new BitVector(1337));
            frame.WriteArgument(1, new BitVector(1338));
            frame.WriteArgument(2, new BitVector(1339));

            Assert.Equal(1337, frame.ReadArgument(0).AsSpan().I32);
            Assert.Equal(1338, frame.ReadArgument(1).AsSpan().I32);
            Assert.Equal(1339, frame.ReadArgument(2).AsSpan().I32);
            
            long address = frame.Allocate(100);
            
            Assert.True(frame.ReadArgument(0).IsFullyKnown);
            Assert.Equal(1337, frame.ReadArgument(0).AsSpan().I32);
            Assert.True(frame.ReadArgument(1).IsFullyKnown);
            Assert.Equal(1338, frame.ReadArgument(1).AsSpan().I32);
            Assert.True(frame.ReadArgument(2).IsFullyKnown);
            Assert.Equal(1339, frame.ReadArgument(2).AsSpan().I32);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(0x7fff_0000)]
        public void AllocateShouldRetainLocals(long baseAddress)
        {
            var method = _fixture.GetTestMethod(nameof(TestClass.MultipleLocals));
            var stack = new CallStack(1000, _factory);
            stack.Rebase(baseAddress);
            
            var frame = stack.Push(method);
            frame.WriteLocal(0, new BitVector(1337));
            frame.WriteLocal(1, new BitVector(1338));
            frame.WriteLocal(2, new BitVector(1339));

            Assert.Equal(1337, frame.ReadLocal(0).AsSpan().I32);
            Assert.Equal(1338, frame.ReadLocal(1).AsSpan().I32);
            Assert.Equal(1339, frame.ReadLocal(2).AsSpan().I32);
            
            long address = frame.Allocate(100);
            
            Assert.True(frame.ReadLocal(0).IsFullyKnown);
            Assert.Equal(1337, frame.ReadLocal(0).AsSpan().I32);
            Assert.True(frame.ReadLocal(1).IsFullyKnown);
            Assert.Equal(1338, frame.ReadLocal(1).AsSpan().I32);
            Assert.True(frame.ReadLocal(2).IsFullyKnown);
            Assert.Equal(1339, frame.ReadLocal(2).AsSpan().I32);
        }
        
    }
}