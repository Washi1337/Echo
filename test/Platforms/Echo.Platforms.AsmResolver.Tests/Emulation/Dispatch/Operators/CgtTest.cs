using AsmResolver.PE.DotNet.Cil;
using Echo.Core;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Operators
{
    public class CgtTest : DispatcherTestBase
    {
        public CgtTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Theory]
        [InlineData(CilCode.Cgt, 0, 0, false)]
        [InlineData(CilCode.Cgt_Un, 0, 0, false)]
        [InlineData(CilCode.Cgt, 1, 0, true)]
        [InlineData(CilCode.Cgt_Un, 1, 0, true)]
        [InlineData(CilCode.Cgt, -1, 0, false)]
        [InlineData(CilCode.Cgt_Un, -1, 0, true)]
        public void I4Comparison(CilCode code, int a, int b, bool? expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I4Value(a));
            stack.Push(new I4Value(b));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ((I4Value) stack.Top).IsNonZero);
        }

        [Theory]
        [InlineData(CilCode.Cgt, 0, 0, false)]
        [InlineData(CilCode.Cgt_Un, 0, 0, false)]
        [InlineData(CilCode.Cgt, 1, 0, true)]
        [InlineData(CilCode.Cgt_Un, 1, 0, true)]
        [InlineData(CilCode.Cgt, -1, 0, false)]
        [InlineData(CilCode.Cgt_Un, -1, 0, true)]
        public void I8Comparison(CilCode code, long a, long b, bool? expected)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(new I8Value(a));
            stack.Push(new I8Value(b));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(expected, ((I4Value) stack.Top).IsNonZero);
        }

        [Theory]
        [InlineData(CilCode.Cgt, 0, 0, false)]
        [InlineData(CilCode.Cgt_Un, 0, 0, false)]
        [InlineData(CilCode.Cgt, 1, 0, true)]
        [InlineData(CilCode.Cgt_Un, 1, 0, true)]
        [InlineData(CilCode.Cgt, double.NaN, 0, false)]
        [InlineData(CilCode.Cgt, 0, double.NaN, false)]
        [InlineData(CilCode.Cgt, double.NaN, double.NaN, false)]
        [InlineData(CilCode.Cgt_Un, double.NaN, 0, true)]
        [InlineData(CilCode.Cgt_Un, 0, double.NaN, true)]
        [InlineData(CilCode.Cgt_Un, double.NaN, double.NaN, true)]
        public void FloatComparison(CilCode code, double a, double b, bool? expected)
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

            var stringValue = environment.MemoryAllocator.GetStringValue("Hello, world!");
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

            var stringValue = environment.MemoryAllocator.GetStringValue("Hello, world!");
            stack.Push(OValue.Null(environment.Is32Bit));
            stack.Push(marshaller.ToCliValue(stringValue, environment.Module.CorLibTypeFactory.String));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Cgt_Un));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(Trilean.True, ((I4Value) stack.Top).IsZero);
        }
        
    }
}