using System;
using System.Linq;
using AsmResolver.DotNet;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using InlineIL;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Stack
{
    public class VirtualFrameTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly ValueFactory _factory;

        public VirtualFrameTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            _factory = new ValueFactory(fixture.CurrentTestModule, false);
        }

        private MethodDefinition GetTestMethod(string methodName) => _fixture.CurrentTestModule
            .TopLevelTypes.First(t => t.Name == nameof(VirtualFrameTest))
            .NestedTypes[0]
            .Methods.First(m => m.Name == methodName);

        [Theory]
        [InlineData(nameof(DummyType.NoLocalsNoArguments), 8)]
        [InlineData(nameof(DummyType.SingleArgument), 8 + 1 * 8)]
        [InlineData(nameof(DummyType.MultipleArguments), 8 + 3 * 8)]
        [InlineData(nameof(DummyType.SingleLocal), 1 * 8 + 8)]
        [InlineData(nameof(DummyType.MultipleLocals), 3 * 8 + 8)]
        [InlineData(nameof(DummyType.MultipleLocalsMultipleArguments), 3 * 8 + 8 + 3 * 8)]
        public void FrameShouldAllocateSufficientSize(string name, int expected)
        {
            var method = GetTestMethod(name);
            var frame = new VirtualFrame(method, _factory);
            Assert.Equal(expected, frame.AddressRange.Length);
        }

        [Theory]
        [InlineData(nameof(DummyType.MultipleLocals), "00000000000000000000000000000000")]
        [InlineData(nameof(DummyType.MultipleLocalsNoInit), "????????????????????????????????")]
        public void ReadLocalTest(string name, string expected)
        {
            var method = GetTestMethod(name);
            var frame = new VirtualFrame(method, _factory);

            var buffer = new BitVector(32, false).AsSpan();
            frame.ReadLocal(0, buffer);
            Assert.Equal(expected, buffer.ToBitString());
        } 

        [Fact]
        public void WriteLocalTest()
        {
            var method = GetTestMethod(nameof(DummyType.MultipleLocals));
            var frame = new VirtualFrame(method, _factory);

            var readBuffer = new BitVector(32, false).AsSpan();
            frame.ReadLocal(0, readBuffer);
            Assert.Equal("00000000000000000000000000000000", readBuffer.ToBitString());

            const uint number = 0b10101010_11001100_11110000_11111111;
            
            var writeBuffer = new BitVector(BitConverter.GetBytes(number)).AsSpan();
            frame.WriteLocal(0, writeBuffer);
            
            frame.ReadLocal(0, readBuffer);
            Assert.Equal("10101010110011001111000011111111", readBuffer.ToBitString());
        } 

        [Fact]
        public void ReadArgumentTest()
        {
            var method = GetTestMethod(nameof(DummyType.MultipleArguments));
            var frame = new VirtualFrame(method, _factory);

            var buffer = new BitVector(32, false).AsSpan();
            frame.ReadArgument(0, buffer);
            Assert.Equal("????????????????????????????????", buffer.ToBitString());
        } 

        [Fact]
        public void WriteArgumentTest()
        {
            var method = GetTestMethod(nameof(DummyType.MultipleArguments));
            var frame = new VirtualFrame(method, _factory);

            var readBuffer = new BitVector(32, false).AsSpan();
            frame.ReadArgument(0, readBuffer);
            Assert.Equal("????????????????????????????????", readBuffer.ToBitString());

            const uint number = 0b10101010_11001100_11110000_11111111;
            
            var writeBuffer = new BitVector(BitConverter.GetBytes(number)).AsSpan();
            frame.WriteArgument(0, writeBuffer);
            
            frame.ReadArgument(0, readBuffer);
            Assert.Equal("10101010110011001111000011111111", readBuffer.ToBitString());
        } 
            
        private sealed class DummyType
        {
            public static void NoLocalsNoArguments()
            {
            }
            
            public static void SingleArgument(int x)
            {
                Console.WriteLine(x);
            }
            
            public static void MultipleArguments(int x, int y, int z)
            {
                Console.WriteLine(x);
                Console.WriteLine(y);
                Console.WriteLine(z);
            }
            
            public static void SingleLocal()
            {
                IL.DeclareLocals(new LocalVar(new TypeRef(typeof(int))));
                IL.Emit.Ldc_I4_0();
                IL.Emit.Stloc_0();
                IL.Emit.Ret();
            }
            
            public static void MultipleLocals()
            {
                IL.DeclareLocals(
                    new LocalVar(new TypeRef(typeof(int))),
                    new LocalVar(new TypeRef(typeof(int))),
                    new LocalVar(new TypeRef(typeof(int)))
                );
                IL.Emit.Ret();
            }
            
            public static void MultipleLocalsNoInit()
            {
                IL.DeclareLocals(
                    false,
                    new LocalVar(new TypeRef(typeof(int))),
                    new LocalVar(new TypeRef(typeof(int))),
                    new LocalVar(new TypeRef(typeof(int)))
                );
                IL.Emit.Ret();
            }
            
            public static void MultipleLocalsMultipleArguments(int a, int b, int c)
            {
                Console.WriteLine(a);
                Console.WriteLine(b);
                Console.WriteLine(c);

                IL.DeclareLocals(
                    new LocalVar(new TypeRef(typeof(int))),
                    new LocalVar(new TypeRef(typeof(int))),
                    new LocalVar(new TypeRef(typeof(int)))
                );
                IL.Emit.Ret();
            }
            
        }
    }
    
    
}