using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class ShrTest : DispatcherTestBase
    {
        public ShrTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Fact]
        public void ShiftSignedI4()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(unchecked((int) 0b11111111_00001111_00110011_01010101)));
            stack.Push(new I4Value(1));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Shr));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(unchecked((int) 0b11111111_10000111_10011001_10101010)), stack.Top);
        }

        [Fact]
        public void ShiftSignedI8()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(
                unchecked((long) 0b11111111_00001111_00110011_01010101_00001111_00110011_01010101_00000000)));
            stack.Push(new I4Value(1));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Shr));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I8Value(
                    unchecked((long) 0b11111111_10000111_10011001_10101010_10000111_10011001_10101010_10000000)),
                stack.Top);
        }

        [Fact]
        public void ShiftUnsignedI4()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(unchecked((int) 0b11111111_00001111_00110011_01010101)));
            stack.Push(new I4Value(1));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Shr_Un));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(0b01111111_10000111_10011001_10101010), stack.Top);
        }

        [Fact]
        public void ShiftUnsignedI8()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I8Value(
                unchecked((long) 0b11111111_00001111_00110011_01010101_00001111_00110011_01010101_00000000)));
            stack.Push(new I4Value(1));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Shr_Un));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I8Value(0b01111111_10000111_10011001_10101010_10000111_10011001_10101010_10000000),
                stack.Top);
        }
    }
}