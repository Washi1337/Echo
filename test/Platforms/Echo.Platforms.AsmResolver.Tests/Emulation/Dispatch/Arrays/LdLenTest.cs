using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arrays
{
    public class LdLenTest : DispatcherTestBase
    {
        public LdLenTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(123)]
        public void LdLenOnValueTypeArray(int length)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            var stack = ExecutionContext.ProgramState.Stack;
            
            var array = environment.MemoryAllocator.AllocateArray(environment.Module.CorLibTypeFactory.Int32, length);
            stack.Push(marshaller.ToCliValue(array, array.Type));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldlen));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new NativeIntegerValue(length, environment.Is32Bit), stack.Top);
        }

        [Fact]
        public void LdLenOnNullShouldThrowNullReferenceException()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(OValue.Null(environment.Is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldlen));
            
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<NullReferenceException>(result.Exception);
        }

        [Fact]
        public void LdLenOnNonArrayObjectRefShouldNotThrow()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;

            var stringValue = environment.MemoryAllocator.GetStringValue("Hello, world!");
            stack.Push(environment.CliMarshaller.ToCliValue(stringValue, environment.Module.CorLibTypeFactory.String));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldlen));
            
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void LdLenOnValueTypeShouldThrowInvalidProgram()
        {
            var stack = ExecutionContext.ProgramState.Stack;

            stack.Push(new I4Value(1234));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldlen));
            
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<InvalidProgramException>(result.Exception);
        }
    }
}