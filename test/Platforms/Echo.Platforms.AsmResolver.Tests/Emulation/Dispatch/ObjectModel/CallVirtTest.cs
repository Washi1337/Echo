using System;
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
    public class CallVirtTest : DispatcherTestBase
    {
        private readonly TypeDefinition _type;

        public CallVirtTest(MockModuleFixture moduleFixture)
            : base(moduleFixture)
        {
            var module = ModuleDefinition.FromFile(typeof(CallTest).Assembly.Location);
            _type = (TypeDefinition) module.LookupMember(typeof(SimpleClass).MetadataToken);
        }

        [Fact]
        public void CallVirtOnNullReferenceShouldThrow()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;
            
            stack.Push(OValue.Null(environment.Is32Bit));

            var method = _type.Methods.First(m => m.Name == nameof(SimpleClass.InstanceMethod));
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Callvirt, method));
            
            Assert.False(result.IsSuccess);
            Assert.IsAssignableFrom<NullReferenceException>(result.Exception);
        }

        [Fact]
        public void CallVirtNonVirtualMethod()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;

            // Create object instance of type and push.
            var objectType = _type.ToTypeSignature();
            var objectRef = new ObjectReference(
                new HleObjectValue(objectType, environment.Is32Bit),
                environment.Is32Bit);
            stack.Push(environment.CliMarshaller.ToCliValue(objectRef, objectType));

            // Call non-virtual method using virtual dispatch.
            var method = _type.Methods.First(m => m.Name == nameof(SimpleClass.InstanceMethod));
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Callvirt, method));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(method, ((HookedMethodInvoker) environment.MethodInvoker).LastInvokedMethod);
        }

        [Fact]
        public void CallVirtOverridedMethod()
        {
            var environment = ExecutionContext.GetService<ICilRuntimeEnvironment>();
            var stack = ExecutionContext.ProgramState.Stack;

            // Create instance of the derived type.
            var objectTypeDef = (TypeDefinition) _type.Module.LookupMember(typeof(DerivedSimpleClass).MetadataToken);
            var objectTypeSig = objectTypeDef.ToTypeSignature();
            
            var objectRef = new ObjectReference(
                new HleObjectValue(objectTypeSig, environment.Is32Bit),
                environment.Is32Bit);
            
            // Push object.
            stack.Push(environment.CliMarshaller.ToCliValue(objectRef, objectTypeSig));

            // Invoke base method using virtual dispatch.
            var method = _type.Methods.First(m => m.Name == nameof(SimpleClass.VirtualInstanceMethod));
            var result = Dispatcher.Execute(ExecutionContext, new CilInstruction(CilOpCodes.Callvirt, method));
            
            Assert.True(result.IsSuccess);
            Assert.Equal(
                objectTypeDef.Methods.First(m => m.Name == method.Name),
                ((HookedMethodInvoker) environment.MethodInvoker).LastInvokedMethod);
        }
    }
}