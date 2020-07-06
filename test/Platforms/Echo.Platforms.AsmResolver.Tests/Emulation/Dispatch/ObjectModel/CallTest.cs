using System.Linq;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Values.ReferenceType;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch.ObjectModel
{
    public class CallTest : DispatcherTestBase
    {
        private readonly TypeDefinition _type;
        
        public CallTest(MockModuleProvider moduleProvider)
            : base(moduleProvider)
        {
            var module = ModuleDefinition.FromFile(typeof(CallTest).Assembly.Location);
            _type = (TypeDefinition) module.LookupMember(typeof(CallTest).MetadataToken);
        }

        [Fact]
        public void CallVoidMethodShouldNotPush()
        {
            var method = _type.Methods.First(m => m.Name == nameof(StaticParameterlessVoidMethod));
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Call, method));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(0, ExecutionContext.ProgramState.Stack.Size);
        }

        [Fact]
        public void CallInt32MethodShouldPushI4()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            
            var method = _type.Methods.First(m => m.Name == nameof(StaticParameterlessInt32Method));
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Call, method));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(1, stack.Size);
            Assert.IsAssignableFrom<I4Value>(stack.Top);
        }

        [Fact]
        public void CallStaticParameterlessMethodShouldNotPop()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            var i4 = new I4Value(1234);
            stack.Push(i4);
            
            var method = _type.Methods.First(m => m.Name == nameof(StaticParameterlessVoidMethod));
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Call, method));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(new[]
            {
                i4
            }, stack.GetAllStackSlots());
        }

        [Fact]
        public void CallStaticParameterizedMethodShouldPopAllArguments()
        {
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(new I4Value(0x1234));
            stack.Push(new I4Value(0x5678));
            stack.Push(new I4Value(0x9abc));

            var method = _type.Methods.First(m => m.Name == nameof(StaticParameterizedMethod));
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.Equal(new []
            {
                new I4Value(0x1234), 
            }, stack.GetAllStackSlots());
        }

        [Fact]
        public void CallInstanceMethodShouldPopObject()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var objectType = _type.ToTypeSignature();
            var objectRef = new ObjectReference(
                new HighLevelObjectValue(objectType, environment.Is32Bit),
                environment.Is32Bit);
            
            var stack = ExecutionContext.ProgramState.Stack;
            stack.Push(environment.CliMarshaller.ToCliValue(objectRef, objectType));

            var method = _type.Methods.First(m => m.Name == nameof(InstanceParameterlessVoidMethod));
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Call, method));

            Assert.True(result.IsSuccess);
            Assert.Equal(0, stack.Size);
        }

        private void InstanceParameterlessVoidMethod()
        {
        }

        private static void StaticParameterlessVoidMethod()
        {
        }

        private static void StaticParameterizedMethod(int x, int y)
        {
        }
        
        private static int StaticParameterlessInt32Method()
        {
            return 1;
        } 
    }
}