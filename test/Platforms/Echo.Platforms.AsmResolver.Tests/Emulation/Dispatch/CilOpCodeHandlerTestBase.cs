using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch
{
    public abstract class CilOpCodeHandlerTestBase : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;

        protected CilOpCodeHandlerTestBase(MockModuleFixture fixture)
        {
            _fixture = fixture;

            var dummyMethod = new MethodDefinition(
                "DummyMethod", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(fixture.MockModule.CorLibTypeFactory.Void));

            var body = new CilMethodBody(dummyMethod);
            body.Instructions.Add(CilOpCodes.Ret);
            
            var vm = new CilVirtualMachine(fixture.MockModule, false);
            vm.CallStack.Push(dummyMethod);
            
            Context = new CilExecutionContext(vm, CancellationToken.None);
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
    }
}