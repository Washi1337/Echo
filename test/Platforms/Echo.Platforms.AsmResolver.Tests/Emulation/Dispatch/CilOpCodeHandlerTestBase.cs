using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch
{
    public abstract class CilOpCodeHandlerTestBase : IClassFixture<MockModuleFixture>
    {
        protected CilOpCodeHandlerTestBase(MockModuleFixture fixture)
        {
            ModuleFixture = fixture;

            var dummyMethod = new MethodDefinition(
                "DummyMethod", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(fixture.MockModule.CorLibTypeFactory.Void));

            var body = new CilMethodBody(dummyMethod);
            body.Instructions.Add(CilOpCodes.Ret);
            
            var vm = new CilVirtualMachine(fixture.MockModule, false);
            var thread = vm.CreateThread();
            thread.CallStack.Push(dummyMethod);
            
            Context = new CilExecutionContext(thread, CancellationToken.None);
            Dispatcher = new CilDispatcher();
        }

        protected CilDispatcher Dispatcher
        {
            get;
        }

        protected CilExecutionContext Context
        {
            get;
        }

        public MockModuleFixture ModuleFixture
        {
            get;
        }
    }
}