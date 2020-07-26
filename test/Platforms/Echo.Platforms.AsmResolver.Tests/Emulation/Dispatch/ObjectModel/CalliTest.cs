using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class CalliTest : DispatcherTestBase
    {
        public CalliTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
        }
        [Fact]
        public void CalliNonVoidPushInt32UnknownValue()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var module = ModuleDefinition.FromFile(typeof(CalliTest).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(TestClass).MetadataToken);
            var method = type.Methods.First(q => q.Name == nameof(TestClass.Calli));

            var sig = method.Signature;
            
            ExecutionContext.ProgramState.Stack.Push(new I4Value(1312));
            
            var functionPointer = typeof(TestClass).GetMethods()
                .First(q => q.Name == nameof(TestClass.Calli)).MethodHandle
                .GetFunctionPointer();
            
            ExecutionContext.ProgramState.Stack.Push(new NativeIntegerValue(functionPointer.ToInt64(),environment.Is32Bit));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Calli, new StandAloneSignature(sig)));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(1,ExecutionContext.ProgramState.Stack.Size);
            Assert.False(ExecutionContext.ProgramState.Stack.Top.IsKnown);
            Assert.IsAssignableFrom<I4Value>(ExecutionContext.ProgramState.Stack.Top);
        }
        
        [Fact]
        public void CallVoidPushNoValue()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var module = ModuleDefinition.FromFile(typeof(CalliTest).Assembly.Location);
            var type = (TypeDefinition) module.LookupMember(typeof(TestClass).MetadataToken);
            var method = type.Methods.First(q => q.Name == nameof(TestClass.CalliVoid));

            var sig = method.Signature;
            
            ExecutionContext.ProgramState.Stack.Push(new I4Value(1312));
            
            var functionPointer = typeof(TestClass).GetMethods()
                .First(q => q.Name == nameof(TestClass.CalliVoid)).MethodHandle
                .GetFunctionPointer();
            
            ExecutionContext.ProgramState.Stack.Push(new NativeIntegerValue(functionPointer.ToInt64(),environment.Is32Bit));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Calli, new StandAloneSignature(sig)));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0,ExecutionContext.ProgramState.Stack.Size);

        }

        private static class TestClass
        {
            public static int Calli(int arg1)
            {
                return arg1;
            }

            public static void CalliVoid(int arg1)
            {
            }
        }
    }
}