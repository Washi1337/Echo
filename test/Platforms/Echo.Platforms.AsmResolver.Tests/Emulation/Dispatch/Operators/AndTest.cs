using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class AndTest : DispatcherTestBase
    {
        public AndTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Fact]
        public void AndI4()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0b00110011));
            stack.Push(new I4Value(0b00001111));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.And));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(0b00000011), stack.Top);
        }

        [Fact]
        public void AndI8()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(0b00110011));
            stack.Push(new I8Value(0b00001111));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.And));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I8Value(0b00000011), stack.Top);
        }

        [Fact]
        public void AndI4WithNativeInteger()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0b00110011));
            stack.Push(new NativeIntegerValue(0b00001111, is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.And));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new NativeIntegerValue(0b00000011, is32Bit), stack.Top);
        }

        [Fact]
        public void AndMismatchIntegers()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0));
            stack.Push(new I8Value(1));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.And));
            Assert.False(result.IsSuccess);
        }
    }
}