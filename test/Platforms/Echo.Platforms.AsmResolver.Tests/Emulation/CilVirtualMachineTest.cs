using System;
using System.Linq;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;
using Xunit.Abstractions;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;
using MethodDefinition = AsmResolver.DotNet.MethodDefinition;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class CilVirtualMachineTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly CilVirtualMachine _vm;

        public CilVirtualMachineTest(MockModuleFixture fixture, ITestOutputHelper testOutputHelper)
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
                body.Instructions.Add(Nop);
            body.Instructions.Add(Ret);
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
                body.Instructions.Add(Nop);
            body.Instructions.Add(Ret);
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
                body.Instructions.Add(Nop);
            body.Instructions.Add(Br, body.Instructions[0].CreateLabel());

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

            var body = new CilMethodBody(dummyMethod)
            {
                Instructions =
                {
                    Ldc_I4_3,
                    Ldc_I4_4,
                    Add,
                    Ldc_I4_5,
                    Mul
                }
            };
            
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
        public void SummationOverArray()
        {
            // Prepare dummy method.
            var factory = _fixture.MockModule.CorLibTypeFactory;
            var sum = new MethodDefinition(
                "Sum", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Int32, factory.Int32.MakeSzArrayType()));

            var loopStart = new CilInstructionLabel();
            sum.CilMethodBody = new CilMethodBody(sum)
            {
                LocalVariables =
                {
                    new CilLocalVariable(factory.Int32),
                    new CilLocalVariable(factory.Int32),
                },
                Instructions =
                {
                    // i = 0;
                    Ldc_I4_0,       
                    Stloc_0,

                    // sum = 0;
                    Ldc_I4_0,
                    Stloc_1,

                    // loopStart:
                    // sum = sum + array[i];
                    Ldloc_1,
                    Ldarg_0,
                    Ldloc_0,
                    Ldelem_I4,
                    Add,
                    Stloc_1,

                    // i++;
                    Ldloc_0,
                    Ldc_I4_1,
                    Add,
                    Stloc_0,

                    // if (i < array.Length) goto loopStart;
                    Ldloc_0,
                    Ldarg_0,
                    Ldlen,
                    { Blt, loopStart },

                    // return sum;
                    Ldloc_1,
                    Ret
                }
            };
            sum.CilMethodBody.Instructions.CalculateOffsets();
            loopStart.Instruction = sum.CilMethodBody.Instructions[4];

            // Set up test array.
            long arrayAddress = _vm.Heap.AllocateSzArray(factory.Int32, 10, false);
            var arraySpan = _vm.Heap.GetObjectSpan(arrayAddress);
            for (int i = 0; i < 10; i++)
                arraySpan.SliceArrayElement(_vm.ValueFactory, factory.Int32, i).Write(100 + i);
            
            // Call Sum.
            var frame = _vm.CallStack.Push(sum);
            frame.WriteArgument(0, new BitVector(arrayAddress));
            _vm.Run();
            
            var returnValue = _vm.CallStack.Peek().EvaluationStack.Peek();
            Assert.Equal(Enumerable.Range(100, 10).Sum(), returnValue.Contents.AsSpan().I32);
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
            var fooBody = new CilMethodBody(foo)
            {
                Instructions =
                {
                    Ldc_I4_3,
                    Ldc_I4_4,
                    { Call, bar },
                    Ldc_I4_5,
                    Mul,
                    Ret,
                }
            };
            fooBody.Instructions.CalculateOffsets();
            foo.CilMethodBody = fooBody;
            
            // return arg0 + arg1;
            var barBody = new CilMethodBody(bar)
            {
                Instructions =
                {
                    Ldarg_0,
                    Ldarg_1,
                    Add,
                    Ret,
                }
            };
            barBody.Instructions.CalculateOffsets();
            bar.CilMethodBody = barBody;
            
            // Ensure VM is stepping into calls.
            _vm.InvocationStrategy = InvokeExternalStrategy.Instance;

            // Call Foo.
            _vm.CallStack.Push(foo);
            _vm.Run();

            var returnValue = _vm.CallStack.Peek().EvaluationStack.Peek();
            Assert.Equal((3 + 4) * 5, returnValue.Contents.AsSpan().I32);
        }
    }
}