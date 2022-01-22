using System;
using System.Threading;
using System.Threading.Tasks;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;
using MethodDefinition = AsmResolver.DotNet.MethodDefinition;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class CilVirtualMachineTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly CilVirtualMachine _vm;

        public CilVirtualMachineTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            _vm = new CilVirtualMachine(fixture.MockModule, false);
        }

        [Fact]
        public void SingleStep()
        {
            // Prepare dummy method.
            var dummyMethod = new MethodDefinition(
                "DummyMethod", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));
            
            var body = new CilMethodBody(dummyMethod);
            for (int i = 0; i < 100; i++)
                body.Instructions.Add(CilOpCodes.Nop);
            body.Instructions.Add(CilOpCodes.Ret);
            body.Instructions.CalculateOffsets();
            dummyMethod.CilMethodBody = body;

            // Push frame on stack.
            _vm.CallStack.Push(dummyMethod);

            // Execute all nops.
            for (int i = 0; i < 100; i++)
                _vm.Step();

            // Check if we're still in the dummy method.
            Assert.NotEmpty(_vm.CallStack);
            
            // Execute return.
            _vm.Step();
            
            // Check if we exited.
            Assert.Empty(_vm.CallStack);
        }

        [Fact(Timeout = 5000)]
        public void RunShouldTerminate()
        {
            // Prepare dummy method.
            var dummyMethod = new MethodDefinition(
                "DummyMethod", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));
            
            var body = new CilMethodBody(dummyMethod);
            for (int i = 0; i < 100; i++)
                body.Instructions.Add(CilOpCodes.Nop);
            body.Instructions.Add(CilOpCodes.Ret);
            body.Instructions.CalculateOffsets();
            dummyMethod.CilMethodBody = body;
            
            // Push frame on stack.
            _vm.CallStack.Push(dummyMethod);

            _vm.Run();
            
            // Check if we exited.
            Assert.Empty(_vm.CallStack);
        }

        [Fact(Timeout = 5000)]
        public void CancelShouldThrow()
        {
            // Prepare dummy method.
            var dummyMethod = new MethodDefinition(
                "DummyMethod", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));
            
            var body = new CilMethodBody(dummyMethod);
            for (int i = 0; i < 100; i++)
                body.Instructions.Add(CilOpCodes.Nop);
            body.Instructions.Add(CilOpCodes.Br, body.Instructions[0].CreateLabel());

            body.Instructions.CalculateOffsets();
            dummyMethod.CilMethodBody = body;
            
            // Push frame on stack.
            _vm.CallStack.Push(dummyMethod);

            var tokenSource = new CancellationTokenSource();
            
            int dispatchCounter = 0;
            _vm.Dispatcher.BeforeInstructionDispatch += (_, _) =>
            {
                dispatchCounter++;
                if (dispatchCounter == 300)
                    tokenSource.Cancel();
            };

            Assert.Throws<OperationCanceledException>(() => _vm.Run(tokenSource.Token));;
        }
    }
}