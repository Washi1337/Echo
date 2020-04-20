using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class ShlTest : DispatcherTestBase
    {
        public ShlTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void ShiftInteger32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(0b00110011));
            stack.Push(new Integer32Value(1));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Shl));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(0b01100110), stack.Top);
        }

        [Fact]
        public void ShiftInteger64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer64Value(0b00110011));
            stack.Push(new Integer32Value(1));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Shl));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer64Value(0b01100110), stack.Top);
        }

        [Fact]
        public void ShiftNativeInteger()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new NativeIntegerValue(0b00110011, is32Bit));
            stack.Push(new Integer32Value(1));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Shl));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new NativeIntegerValue(0b01100110, is32Bit), stack.Top);
        }
    }
}