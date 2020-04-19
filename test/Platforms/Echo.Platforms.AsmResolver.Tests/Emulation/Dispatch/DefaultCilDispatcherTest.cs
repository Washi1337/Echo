using System;
using System.ComponentModel.Design;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ValueType;
using Echo.Core.Code;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch
{
    public class DefaultCilDispatcherTest : IClassFixture<MockModuleProvider>
    {
        private readonly DefaultCilDispatcher _dispatcher;
        private readonly ExecutionContext _context;

        public DefaultCilDispatcherTest(MockModuleProvider moduleProvider)
        {
            _dispatcher = new DefaultCilDispatcher();

            var dummyModule = moduleProvider.GetModule();

            var dummyMethod = new MethodDefinition(
                "MockMethod",
                MethodAttributes.Static,
                MethodSignature.CreateStatic(dummyModule.CorLibTypeFactory.Void));
            dummyMethod.CilMethodBody = new CilMethodBody(dummyMethod);
            
            var container = new ServiceContainer();
            container.AddService(typeof(ICilRuntimeEnvironment), new MockCilRuntimeEnvironment
            {
                Is32Bit = false,
                Architecture = new CilArchitecture(dummyMethod.CilMethodBody)
            });
            
            _context = new ExecutionContext(container, new CilProgramState(), default);
        }
        
        [Fact]
        public void Nop()
        {
            var result = _dispatcher.Execute(_context, new CilInstruction(CilOpCodes.Nop));
            Assert.True(result.IsSuccess);
        }
        
        [Fact]
        public void Ret()
        {
            var result = _dispatcher.Execute(_context, new CilInstruction(CilOpCodes.Ret));
            Assert.True(result.IsSuccess);
            Assert.True(result.Exit);
        }
        
        [Fact]
        public void LdcI4()
        {
            var result = _dispatcher.Execute(_context, new CilInstruction(CilOpCodes.Ldc_I4, 1234));
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(1234), _context.ProgramState.Stack.Top);
        }
        
        [Fact]
        public void LdcI4S()
        {
            var result = _dispatcher.Execute(_context, new CilInstruction(CilOpCodes.Ldc_I4_S, (sbyte) -128));
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(-128), _context.ProgramState.Stack.Top);
        }
        
        [Theory]
        [InlineData(CilCode.Ldc_I4_0, 0)]
        [InlineData(CilCode.Ldc_I4_1, 1)]
        [InlineData(CilCode.Ldc_I4_2, 2)]
        [InlineData(CilCode.Ldc_I4_3, 3)]
        [InlineData(CilCode.Ldc_I4_4, 4)]
        [InlineData(CilCode.Ldc_I4_5, 5)]
        [InlineData(CilCode.Ldc_I4_6, 6)]
        [InlineData(CilCode.Ldc_I4_7, 7)]
        [InlineData(CilCode.Ldc_I4_8, 8)]
        [InlineData(CilCode.Ldc_I4_M1, -1)]
        public void LdcI4Macro(CilCode code, int expected)
        {
            var result = _dispatcher.Execute(_context, new CilInstruction(code.ToOpCode()));
            Assert.True(result.IsSuccess);
            Assert.Equal(new Integer32Value(expected), _context.ProgramState.Stack.Top);
        }

        [Fact]
        public void LdcR4()
        {
            var result = _dispatcher.Execute(_context, new CilInstruction(CilOpCodes.Ldc_R4, 1.23f));
            Assert.True(result.IsSuccess);
            Assert.Equal(new Float32Value(1.23f), _context.ProgramState.Stack.Top);
        }

        [Fact]
        public void LdcR8()
        {
            var result = _dispatcher.Execute(_context, new CilInstruction(CilOpCodes.Ldc_R8, 1.23D));
            Assert.True(result.IsSuccess);
            Assert.Equal(new Float64Value(1.23D), _context.ProgramState.Stack.Top);
        }

        [Fact]
        public void Pop()
        {
            _context.ProgramState.Stack.Push(new UnknownValue());
            var result = _dispatcher.Execute(_context, new CilInstruction(CilOpCodes.Pop));
            Assert.True(result.IsSuccess);
            Assert.Equal(0, _context.ProgramState.Stack.Size);
        }

        [Fact]
        public void DupWithValueType()
        {
            var value = new Integer32Value(1234);
            _context.ProgramState.Stack.Push(value);
            
            var result = _dispatcher.Execute(_context, new CilInstruction(CilOpCodes.Dup));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(2, _context.ProgramState.Stack.Size);
            Assert.All(
                _context.ProgramState.Stack.GetAllStackSlots(), 
                v => Assert.Equal(v, value));
        }
    }
}