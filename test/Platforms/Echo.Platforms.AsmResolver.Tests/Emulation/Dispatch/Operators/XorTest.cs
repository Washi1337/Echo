using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class XorTest : DispatcherTestBase
    {
        public XorTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void XorI4()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0b00110011));
            stack.Push(new I4Value(0b00001111));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Xor));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(0b00111100), stack.Top);
        }
        
        [Fact]
        public void XorI8()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(0b00110011));
            stack.Push(new I8Value(0b00001111));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Xor));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I8Value(0b00111100), stack.Top);
        }

        [Fact]
        public void XorI4WithNativeInteger()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0b00110011));
            stack.Push(new NativeIntegerValue(0b00001111, is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Xor));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new NativeIntegerValue(0b00111100, is32Bit), stack.Top);
        }

        [Fact]
        public void XorMismatchIntegers()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0));
            stack.Push(new I8Value(1));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Xor));
            Assert.False(result.IsSuccess);
        }
    }
}