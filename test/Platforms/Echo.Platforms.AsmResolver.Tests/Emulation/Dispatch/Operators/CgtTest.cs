using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;
using static Echo.Core.TrileanValue;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class CgtTest : DispatcherTestBase
    {
        public CgtTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Theory]
        [InlineData(CilCode.Cgt, 0, 0, False)]
        [InlineData(CilCode.Cgt_Un, 0, 0, False)]
        [InlineData(CilCode.Cgt, 1, 0, True)]
        [InlineData(CilCode.Cgt_Un, 1, 0, True)]
        [InlineData(CilCode.Cgt, -1, 0, False)]
        [InlineData(CilCode.Cgt_Un, -1, 0, True)]
        public void I4Comparison(CilCode code, int a, int b, TrileanValue expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I4Value(a));
            stack.Push(new I4Value(b));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ((I4Value) stack.Top).IsNonZero);
        }

        [Theory]
        [InlineData(CilCode.Cgt, 0, 0, False)]
        [InlineData(CilCode.Cgt_Un, 0, 0, False)]
        [InlineData(CilCode.Cgt, 1, 0, True)]
        [InlineData(CilCode.Cgt_Un, 1, 0, True)]
        [InlineData(CilCode.Cgt, -1, 0, False)]
        [InlineData(CilCode.Cgt_Un, -1, 0, True)]
        public void I8Comparison(CilCode code, long a, long b, TrileanValue expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I8Value(a));
            stack.Push(new I8Value(b));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ((I4Value) stack.Top).IsNonZero);
        }

        [Theory]
        [InlineData(CilCode.Cgt, 0, 0, False)]
        [InlineData(CilCode.Cgt_Un, 0, 0, False)]
        [InlineData(CilCode.Cgt, 1, 0, True)]
        [InlineData(CilCode.Cgt_Un, 1, 0, True)]
        [InlineData(CilCode.Cgt, double.NaN, 0, False)]
        [InlineData(CilCode.Cgt, 0, double.NaN, False)]
        [InlineData(CilCode.Cgt, double.NaN, double.NaN, False)]
        [InlineData(CilCode.Cgt_Un, double.NaN, 0, True)]
        [InlineData(CilCode.Cgt_Un, 0, double.NaN, True)]
        [InlineData(CilCode.Cgt_Un, double.NaN, double.NaN, True)]
        public void FloatComparison(CilCode code, double a, double b, TrileanValue expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new FValue(a));
            stack.Push(new FValue(b));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ((I4Value) stack.Top).IsNonZero);
        }

        [Fact]
        public void ObjectWithNullComparison()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var stack = ExecutionContext.ProgramState.Stack;

            var stringValue = environment.ValueFactory.GetStringValue("Hello, world!");
            stack.Push(marshaller.ToCliValue(stringValue, environment.Module.CorLibTypeFactory.String));
            stack.Push(OValue.Null(environment.Is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Cgt_Un));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(Trilean.True, ((I4Value) stack.Top).IsNonZero);
        }

        [Fact]
        public void NullWithObjectComparison()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var stack = ExecutionContext.ProgramState.Stack;

            var stringValue = environment.ValueFactory.GetStringValue("Hello, world!");
            stack.Push(OValue.Null(environment.Is32Bit));
            stack.Push(marshaller.ToCliValue(stringValue, environment.Module.CorLibTypeFactory.String));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Cgt_Un));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(Trilean.True, ((I4Value) stack.Top).IsZero);
        }
        
    }
}