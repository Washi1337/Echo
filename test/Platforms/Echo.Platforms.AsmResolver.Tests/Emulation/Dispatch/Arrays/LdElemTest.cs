using System;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
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
            var stack = ExecutionContext.ProgramState.Stack;
            
            var array = new ArrayValue(new IConcreteValue[]
            {
                new Integer32Value(0xA), new Integer32Value(0xB), new Integer32Value(0xC), new Integer32Value(0xD),  
            });
            stack.Push(array);
            stack.Push(new I4Value(index));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(expectedValue), stack.Top);
        }

        [Theory]
        [InlineData(0, 0xA)]
        [InlineData(1, 0xB)]
        [InlineData(2, 0xC)]
        [InlineData(3, 0xD)]
        public void LdelemI4UsingNativeIntegerIndex(long index, int expectedValue)
        {
            bool is32Bit = ExecutionContext.GetService<ICilRuntimeEnvironment>().Is32Bit;
            
            var stack = ExecutionContext.ProgramState.Stack;
            var array = new ArrayValue(new IConcreteValue[]
            {
                new Integer32Value(0xA), new Integer32Value(0xB), new Integer32Value(0xC), new Integer32Value(0xD),  
            });
            stack.Push(array);
            stack.Push(new NativeIntegerValue(index, is32Bit));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(expectedValue), stack.Top);
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
            var stack = ExecutionContext.ProgramState.Stack;
            var array = new ArrayValue(new IConcreteValue[]
            {
                new Integer32Value(arrayElementValue),
            });
            stack.Push(array);
            stack.Push(new I4Value(0));

            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(code.ToOpCode()));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new I4Value(expectedValue), stack.Top);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        [InlineData(100)]
        public void ArrayAccessOutOfBoundsShouldThrow(int index)
        {
            var stack = ExecutionContext.ProgramState.Stack;
            var array = new ArrayValue(new IConcreteValue[]
            {
                new Integer32Value(0),
                new Integer32Value(1),
                new Integer32Value(2),
            });
            stack.Push(array);
            stack.Push(new I4Value(index));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_I4));
            
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<IndexOutOfRangeException>(result.Exception);
        }

        [Fact]
        public void LdElemR4ShouldExtendToFloat64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            var array = new ArrayValue(new IConcreteValue[]
            {
                new Float32Value(1.23f), 
            });
            stack.Push(array);
            stack.Push(new I4Value(0));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_R4));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new FValue(1.23f), stack.Top);
        }

        [Fact]
        public void LdElemR8CopyFloat64()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            var array = new ArrayValue(new IConcreteValue[]
            {
                new Float64Value(1.23D), 
            });
            stack.Push(array);
            stack.Push(new I4Value(0));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Ldelem_R4));
            
            Assert.True(result.IsSuccess);
            Assert.NotSame(array[0], stack.Top);
            Assert.Equal(new FValue(1.23D), stack.Top);
        }
    }
}