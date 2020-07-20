using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;
using static Echo.Core.TrileanValue;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class CeqTest : DispatcherTestBase
    {
        public CeqTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Theory]
        [InlineData("111000", "111000", True)]
        [InlineData("111000", "000111", False)]
        [InlineData("111000", "11000?", False)]
        [InlineData("111000", "11100?", Unknown)]
        public void CompareI4sOnStack(string a, string b, TrileanValue expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I4Value(a));
            stack.Push(new I4Value(b));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ceq));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ((I4Value) stack.Top).IsNonZero);
        }

        [Theory]
        [InlineData("111000", "111000", True)]
        [InlineData("111000", "000111", False)]
        [InlineData("111000", "11000?", False)]
        [InlineData("111000", "11100?", Unknown)]
        public void CompareI8sOnStack(string a, string b, TrileanValue expected)
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