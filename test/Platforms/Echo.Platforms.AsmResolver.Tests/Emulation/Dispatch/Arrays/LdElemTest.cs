using System;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.Arrays
{
    public class LdElemTest : DispatcherTestBase
    {
        public LdElemTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }

        [Theory]
        [InlineData(0, 0xA)]
        [InlineData(1, 0xB)]
        [InlineData(2, 0xC)]
        [InlineData(3, 0xD)]
        public void LdelemI4UsingI4Index(int index, int expectedValue)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var array = environment.MemoryAllocator.AllocateArray(environment.Module.CorLibTypeFactory.Int32, 4);
            array.StoreElementI4(0, new I4Value(0xA), marshaller);
            array.StoreElementI4(1, new I4Value(0xB), marshaller);
            array.StoreElementI4(2, new I4Value(0xC), marshaller);
            array.StoreElementI4(3, new I4Value(0xD), marshaller);

            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(marshaller.ToCliValue(array, new SzArrayTypeSignature(array.ElementType)));
            stack.Push(new I4Value(index));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.True(result.IsSuccess, $"Unexpected {result.Exception?.GetType()}: {result.Exception?.Message}");
            Assert.Equal(new I4Value(expectedValue), stack.Top);
        }

        [Theory]
        [InlineData(0, 0xA)]
        [InlineData(1, 0xB)]
        [InlineData(2, 0xC)]
        [InlineData(3, 0xD)]
        public void LdelemI4UsingNativeIntegerIndex(long index, int expectedValue)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var array = environment.MemoryAllocator.AllocateArray(environment.Module.CorLibTypeFactory.Int32, 4);
            array.StoreElementI4(0, new I4Value(0xA), marshaller);
            array.StoreElementI4(1, new I4Value(0xB), marshaller);
            array.StoreElementI4(2, new I4Value(0xC), marshaller);
            array.StoreElementI4(3, new I4Value(0xD), marshaller);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(marshaller.ToCliValue(array, new SzArrayTypeSignature(array.ElementType)));
            stack.Push(new NativeIntegerValue(index, environment.Is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.True(result.IsSuccess, $"Unexpected {result.Exception?.GetType()}: {result.Exception?.Message}");
            Assert.Equal(new I4Value(expectedValue), stack.Top);
        }

        [Fact]
        public void LdElemI8OnLastIndexOfInt8ArrayShouldReturnZeroes()
        {
            // NOTE: This is undocumented behaviour.
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var array = environment.MemoryAllocator.AllocateArray(environment.Module.CorLibTypeFactory.Byte, 10);
            for (int i = 0; i < array.Length; i++)
                array.StoreElementU1(i, new I4Value(i), marshaller);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(marshaller.ToCliValue(array, new SzArrayTypeSignature(array.ElementType)));
            stack.Push(new I4Value(9));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I8));
            
            Assert.True(result.IsSuccess, $"Unexpected {result.Exception?.GetType()}: {result.Exception?.Message}");
            Assert.Equal(new I8Value(0), stack.Top);
        }

        [Theory]
        [InlineData(CilCode.Ldelem_I1, 0x00FF0080, -0x80)]
        [InlineData(CilCode.Ldelem_U1, 0x00FF0080, 0x80)]
        [InlineData(CilCode.Ldelem_I1, 0x00FF007F, 0x7F)]
        [InlineData(CilCode.Ldelem_U1, 0x00FF007F, 0x7F)]
        [InlineData(CilCode.Ldelem_I2, 0x0F008000, -0x8000)]
        [InlineData(CilCode.Ldelem_U2, 0x0F008000, 0x8000)]
        [InlineData(CilCode.Ldelem_I2, 0x0F007F00, 0x7F00)]
        [InlineData(CilCode.Ldelem_U2, 0x0F007F00, 0x7F00)]
        [InlineData(CilCode.Ldelem_I4, 0x0F007F00, 0x0F007F00)]
        [InlineData(CilCode.Ldelem_U4, 0x0F007F00, 0x0F007F00)]
        public void LdelemOnInt32ArrayShouldTruncateAndSignExtendWhenNecessary(CilCode code, int arrayElementValue, int expectedValue)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var array = environment.MemoryAllocator.AllocateArray(environment.Module.CorLibTypeFactory.Int32, 1);
            array.StoreElementI4(0, new I4Value(arrayElementValue), marshaller);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(marshaller.ToCliValue(array, new SzArrayTypeSignature(array.ElementType)));
            stack.Push(new I4Value(0));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));
            
            Assert.True(result.IsSuccess, $"Unexpected {result.Exception?.GetType()}: {result.Exception?.Message}");
            Assert.Equal(new I4Value(expectedValue), stack.Top);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(100)]
        public void ArrayAccessOutOfBoundsShouldThrow(int index)
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var array = environment.MemoryAllocator.AllocateArray(environment.Module.CorLibTypeFactory.Int32, 3);
            array.StoreElementI4(0, new I4Value(0), marshaller);
            array.StoreElementI4(1, new I4Value(1), marshaller);
            array.StoreElementI4(2, new I4Value(2), marshaller);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(marshaller.ToCliValue(array, new SzArrayTypeSignature(array.ElementType)));
            stack.Push(new I4Value(index));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<IndexOutOfRangeException>(result.Exception);
        }

        [Fact]
        public void LdElemR4OnSingleArray()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var array = environment.MemoryAllocator.AllocateArray(environment.Module.CorLibTypeFactory.Single, 1);
            array.StoreElementR4(0, new FValue(1.23f), marshaller);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(marshaller.ToCliValue(array, new SzArrayTypeSignature(array.ElementType)));
            stack.Push(new I4Value(0));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_R4));
            
            Assert.True(result.IsSuccess, $"Unexpected {result.Exception?.GetType()}: {result.Exception?.Message}");
            Assert.Equal(new FValue(1.23f), stack.Top);
        }

        [Fact]
        public void LdElemR8OnDoubleArray()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var marshaller = environment.CliMarshaller;
            
            var array = environment.MemoryAllocator.AllocateArray(environment.Module.CorLibTypeFactory.Double, 1);
            array.StoreElementR8(0, new FValue(1.23D), marshaller);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(marshaller.ToCliValue(array, new SzArrayTypeSignature(array.ElementType)));
            stack.Push(new I4Value(0));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_R8));
            
            Assert.True(result.IsSuccess, $"Unexpected {result.Exception?.GetType()}: {result.Exception?.Message}");
            Assert.Equal(new FValue(1.23D), stack.Top);
        }

        [Fact]
        public void LdElemOnNullShouldThrowNullReferenceException()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();

            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(OValue.Null(environment.Is32Bit));
            stack.Push(new I4Value(0));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<NullReferenceException>(result.Exception);
        }

        [Fact]
        public void LdElemOnNonArrayObjectShouldNotThrow()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();

            var stack = ExecutionContext.ProgramState.Stack;
            var stringValue = environment.MemoryAllocator.GetStringValue("Hello, world!");
            stack.Push(environment.CliMarshaller.ToCliValue(stringValue, environment.Module.CorLibTypeFactory.String));
            stack.Push(new I4Value(0));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void LdElemOnValueTypeShouldThrowInvalidProgram()
        {
            var stack = ExecutionContext.ProgramState.Stack;

            stack.Push(new I4Value(1234));
            stack.Push(new I4Value(0));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<InvalidProgramException>(result.Exception);
        }
    }
}