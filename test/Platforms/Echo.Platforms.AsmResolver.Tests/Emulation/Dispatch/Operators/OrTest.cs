using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class OrTest : DispatcherTestBase
    {
        public OrTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void OrI4()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0b00110011));
            stack.Push(new I4Value(0b00001111));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Or));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(0b00111111), stack.Top);
        }
        
        [Fact]
        public void OrI8()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(0b00110011));
            stack.Push(new I8Value(0b00001111));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Or));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I8Value(0b00111111), stack.Top);
        }

        [Fact]
        public void OrI4WithNativeInteger()
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0b00110011));
            stack.Push(new NativeIntegerValue(0b00001111, is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Or));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new NativeIntegerValue(0b00111111, is32Bit), stack.Top);
        }

        [Fact]
        public void OrMismatchIntegers()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0));
            stack.Push(new I8Value(1));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Or));
            Assert.False(result.IsSuccess);
        }
    }
}