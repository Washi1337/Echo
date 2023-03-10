using System;
using System.Threading;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
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
            Assert.Equal(2, _vm.CallStack.Count);
            
            // Execute return.
            _vm.Step();
            
            // Check if we exited.
            Assert.True(Assert.Single(_vm.CallStack).IsRoot);
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
            Assert.True(Assert.Single(_vm.CallStack).IsRoot);
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

        [Fact]
        public void SimpleExpression()
        {
            // Prepare dummy method.
            var dummyMethod = new MethodDefinition(
                "DummyMethod", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));
            
            var body = new CilMethodBody(dummyMethod);
            body.Instructions.Add(CilOpCodes.Ldc_I4_3);
            body.Instructions.Add(CilOpCodes.Ldc_I4_4);
            body.Instructions.Add(CilOpCodes.Add);
            body.Instructions.Add(CilOpCodes.Ldc_I4_5);
            body.Instructions.Add(CilOpCodes.Mul);
            body.Instructions.CalculateOffsets();
            dummyMethod.CilMethodBody = body;
            
            // Push frame on stack.
            var frame = _vm.CallStack.Push(dummyMethod);

            for (int i = 0; i < 5; i++)
                _vm.Step();

            var result = frame.EvaluationStack.Peek();
            Assert.Equal((3 + 4) * 5, result.Contents.AsSpan().I32);
        }

        [Fact]
        public void CallFunction()
        {
            // Prepare dummy methods.
            var factory = _fixture.MockModule.CorLibTypeFactory;
            var foo = new MethodDefinition(
                "Foo", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Int32));

            var bar = new MethodDefinition(
                "Bar",
                MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Int32, factory.Int32, factory.Int32));
            
            // return Bar(3, 4) * 5;
            var fooBody = new CilMethodBody(foo);
            fooBody.Instructions.Add(CilOpCodes.Ldc_I4_3);
            fooBody.Instructions.Add(CilOpCodes.Ldc_I4_4);
            fooBody.Instructions.Add(CilOpCodes.Call, bar);
            fooBody.Instructions.Add(CilOpCodes.Ldc_I4_5);
            fooBody.Instructions.Add(CilOpCodes.Mul);
            fooBody.Instructions.Add(CilOpCodes.Ret);
            fooBody.Instructions.CalculateOffsets();
            foo.CilMethodBody = fooBody;
            
            // return arg0 + arg1;
            var barBody = new CilMethodBody(bar);
            barBody.Instructions.Add(CilOpCodes.Ldarg_0);
            barBody.Instructions.Add(CilOpCodes.Ldarg_1);
            barBody.Instructions.Add(CilOpCodes.Add);
            barBody.Instructions.Add(CilOpCodes.Ret);
            barBody.Instructions.CalculateOffsets();
            bar.CilMethodBody = barBody;
            
            // Ensure VM is stepping into calls.
            _vm.InvocationStrategy = InvokeExternalStrategy.Instance;

            // Call Foo.
            _vm.CallStack.Push(foo);
            
            // Run.
            _vm.Run();

            var returnValue = _vm.CallStack.Peek().EvaluationStack.Peek();
            Assert.Equal((3 + 4) * 5, returnValue.Contents.AsSpan().I32);
        }
    }
}