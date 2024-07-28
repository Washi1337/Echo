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
    public class CilDispatcherTest: IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly CilExecutionContext _context;

        public CilDispatcherTest(MockModuleFixture fixture)
        {
            _fixture = fixture;

            var dummyMethod = new MethodDefinition(
                "DummyMethod",
                MethodAttributes.Static,
                MethodSignature.CreateStatic(fixture.MockModule.CorLibTypeFactory.Void));

            var body = new CilMethodBody(dummyMethod);
            body.Instructions.Add(CilOpCodes.Ret);

            var vm = new CilVirtualMachine(fixture.MockModule, false);
            var thread = vm.CreateThread();
            thread.CallStack.Push(dummyMethod);

            _context = new CilExecutionContext(thread, CancellationToken.None);
        }

        [Fact]
        public void DispatcherTableIsPopulated()
        {
            var dispatcher = new CilDispatcher();
            Assert.NotEmpty(dispatcher.DispatcherTable);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void SetIsHandledInBeforeInstructionDispatchShouldCancelExecution(bool isHandled)
        {
            var handler = new MyOpCodeHandler();
            var dispatcher = new CilDispatcher
            {
                DispatcherTable =
                {
                    [CilCode.Nop] = handler
                }
            };

            dispatcher.BeforeInstructionDispatch += (sender, args) => args.IsHandled = isHandled;
            dispatcher.Dispatch(_context, new CilInstruction(CilOpCodes.Nop));
            Assert.Equal(!isHandled, handler.HasFired);
        }

        private sealed class MyOpCodeHandler : ICilOpCodeHandler
        {
            public bool HasFired
            {
                get;
                private set;
            }
            
            public CilDispatchResult Dispatch(CilExecutionContext context, CilInstruction instruction)
            {
                HasFired = true;
                return CilDispatchResult.Success();
            }
        }
    }
}