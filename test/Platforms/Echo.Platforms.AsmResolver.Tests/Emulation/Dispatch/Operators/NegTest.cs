using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
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
        public void NegInteger32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(0x1234));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Neg));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(-0x1234), stack.Top);
        }

        [Fact]
        public void NegInteger64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer64Value(0x1234));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Neg));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer64Value(-0x1234), stack.Top);
        }
    }
}