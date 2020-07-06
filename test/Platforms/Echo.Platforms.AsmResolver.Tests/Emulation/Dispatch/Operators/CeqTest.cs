using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class CeqTest : DispatcherTestBase
    {
        public CeqTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Theory]
        [InlineData("111000", "111000", true)]
        [InlineData("111000", "000111", false)]
        [InlineData("111000", "11000?", false)]
        [InlineData("111000", "11100?", null)]
        public void CompareI4sOnStack(string a, string b, bool? expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I4Value(a));
            stack.Push(new I4Value(b));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ceq));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ((I4Value) stack.Top).IsNonZero);
        }

        [Theory]
        [InlineData("111000", "111000", true)]
        [InlineData("111000", "000111", false)]
        [InlineData("111000", "11000?", false)]
        [InlineData("111000", "11100?", null)]
        public void CompareI8sOnStack(string a, string b, bool? expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I8Value(a));
            stack.Push(new I8Value(b));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ceq));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ((I4Value) stack.Top).IsNonZero);
        }

    }
}