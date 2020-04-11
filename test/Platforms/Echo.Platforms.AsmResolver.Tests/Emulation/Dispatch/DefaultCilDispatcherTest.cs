using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Values.ValueType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch
{
    public class DefaultCilDispatcherTest
    {
        private readonly DefaultCilDispatcher _dispatcher;
        private readonly ExecutionContext _context;

        public DefaultCilDispatcherTest()
        {
            _dispatcher = new DefaultCilDispatcher();
            _context = new ExecutionContext(new CilProgramState(), default);
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
    }
}