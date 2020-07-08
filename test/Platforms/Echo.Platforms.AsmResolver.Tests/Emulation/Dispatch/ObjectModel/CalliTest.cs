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
        public CalliTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
        }
        [Fact]
        public void CalliNonVoidPushInt32UnknownValue()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var module = ModuleDefinition.FromFile(typeof(CalliTest).Assembly.Location);
            var type = module.TopLevelTypes.First(q => q.Name == nameof(CalliTest));
            var method = type.Methods.First(q => q.Name == "Calli");

            var sig = method.Signature;
            
            ExecutionContext.ProgramState.Stack.Push(new I4Value(1312));
            
            var functionPointer = typeof(CalliTest).GetMethods().First(q => q.Name == "Calli").MethodHandle
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
            var type = module.TopLevelTypes.First(q => q.Name == nameof(CalliTest));
            var method = type.Methods.First(q => q.Name == "Callivoid");

            var sig = method.Signature;
            
            ExecutionContext.ProgramState.Stack.Push(new I4Value(1312));
            
            var functionPointer = typeof(CalliTest).GetMethods().First(q => q.Name == "Callivoid").MethodHandle
                .GetFunctionPointer();
            
            ExecutionContext.ProgramState.Stack.Push(new NativeIntegerValue(functionPointer.ToInt64(),environment.Is32Bit));
            
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Calli, new StandAloneSignature(sig)));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0,ExecutionContext.ProgramState.Stack.Size);

        }

        public static int Calli(int arg1)
        {
            return arg1;
        }
        public static void Callivoid(int arg1)
        {
        }
    }
}