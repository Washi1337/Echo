using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class NotTest : DispatcherTestBase
    {
        public NotTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Fact]
        public void NotI4()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0b00000000_11111111_00001111_11110000));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Not));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(unchecked((int) 0b11111111_00000000_11110000_00001111)), stack.Top);
        }

        [Fact]
        public void NotI8()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(0b00000000_11111111_00001111_11110000_00110011_11001100_01010101_10101010));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Not));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(
                new I8Value(unchecked((long) 0b11111111_00000000_11110000_00001111_11001100_00110011_10101010_01010101)),
                stack.Top);
        }
    }
}