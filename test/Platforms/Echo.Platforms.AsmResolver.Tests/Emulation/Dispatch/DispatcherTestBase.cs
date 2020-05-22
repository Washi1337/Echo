using System.ComponentModel.Design;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete.Emulation;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Values;
using Echo.Platforms.AsmResolver.Emulation.Values.Cli;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch
{
    public class DispatcherTestBase : IClassFixture<MockModuleProvider>
    {
        public DispatcherTestBase(MockModuleProvider moduleProvider)
        {
            const bool is32Bit = false;
            
            Dispatcher = new DefaultCilDispatcher();

            var dummyModule = moduleProvider.GetModule();
            var dummyMethod = new MethodDefinition(
                "MockMethod",
                MethodAttributes.Static,
                MethodSignature.CreateStatic(dummyModule.CorLibTypeFactory.Void));
            dummyMethod.CilMethodBody = new CilMethodBody(dummyMethod);
                
            var environment = new MockCilRuntimeEnvironment
            {
                Is32Bit = is32Bit,
                Architecture = new CilArchitecture(dummyMethod.CilMethodBody),
                Module = dummyModule,
                MemoryAllocator = new DefaultMemoryAllocator(dummyModule, is32Bit),
                MethodInvoker = new HookedMethodInvoker(new ReturnUnknownMethodInvoker(new UnknownValueFactory(is32Bit)))
            };

            var container = new ServiceContainer();
            container.AddService(typeof(ICilRuntimeEnvironment), environment);
            
            ExecutionContext = new ExecutionContext(container, new CilProgramState(), default);
        }

        public ExecutionContext ExecutionContext
        {
            get;
        }

        public DefaultCilDispatcher Dispatcher
        {
            get;
        }
    }
}