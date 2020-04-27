using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class NegTest : DispatcherTestBase
    {
        public NegTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void NegI4()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0x1234));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Neg));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(-0x1234), stack.Top);
        }

        [Fact]
        public void NegI8()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(0x1234));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Neg));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I8Value(-0x1234), stack.Top);
        }
    }
}