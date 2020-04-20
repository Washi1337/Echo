using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class AndTest : DispatcherTestBase
    {
        public AndTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void AndInteger32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(0b00110011));
            stack.Push(new Integer32Value(0b00001111));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.And));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(0b00000011), stack.Top);
        }

        [Fact]
        public void AndInteger64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer64Value(0b00110011));
            stack.Push(new Integer64Value(0b00001111));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.And));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer64Value(0b00000011), stack.Top);
        }

        [Fact]
        public void AndInteger32WithNativeInteger()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(0b00110011));
            stack.Push(new NativeIntegerValue(0b00001111, is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.And));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new NativeIntegerValue(0b00000011, is32Bit), stack.Top);
        }

        [Fact]
        public void AndMismatchIntegers()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(0));
            stack.Push(new Integer64Value(1));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.And));
            Assert.False(result.IsSuccess);
        }
    }
}