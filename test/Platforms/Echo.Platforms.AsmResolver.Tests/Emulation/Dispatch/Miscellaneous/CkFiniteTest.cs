using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Miscellaneous
{
    public class CkFinite : DispatcherTestBase
    {
        public CkFinite(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Theory]
        [InlineData(Double.NaN)]
        [InlineData(Double.PositiveInfinity)]
        [InlineData(Double.NegativeInfinity)]
        public void CkfiniteThrow(double d)
        {
            var value = new FValue(d);
            ExecutionContext.ProgramState.Stack.Push(value);
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ckfinite));
            
            Assert.False(result.IsSuccess);
            Assert.Equal(result.Exception.Message,new ArithmeticException().Message);
            Assert.Equal(1,ExecutionContext.ProgramState.Stack.Size);

        }
        
        [Fact]
        public void CkfiniteNoThrow()
        {
            var value = new FValue(1);
            ExecutionContext.ProgramState.Stack.Push(value);
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ckfinite));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(1,ExecutionContext.ProgramState.Stack.Size);
        }
    }
}