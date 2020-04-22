using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class NotTest : DispatcherTestBase
    {
        public NotTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void NotInteger32()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer32Value(0b00000000_11111111_00001111_11110000));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Not));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(0b11111111_00000000_11110000_00001111), stack.Top);
        }

        [Fact]
        public void NotInteger64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new Integer64Value(0b00000000_11111111_00001111_11110000_00110011_11001100_01010101_10101010));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Not));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(
                new Integer64Value(0b11111111_00000000_11110000_00001111_11001100_00110011_10101010_01010101),
                stack.Top);
        }
    }
}