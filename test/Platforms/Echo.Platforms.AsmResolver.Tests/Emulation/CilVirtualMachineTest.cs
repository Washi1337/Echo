using System;
using System.Linq;
using System.Text;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Concrete;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch;
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
        public void StepMultiple()
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
        public void StepOverNonCallsShouldStepOnce()
        {
            // Prepare dummy method.
            var dummyMethod = new MethodDefinition(
                "DummyMethod", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));

            dummyMethod.CilMethodBody = new CilMethodBody(dummyMethod)
            {
                Instructions =
                {
                    {Ldc_I4, 1337},
                    Ret
                }
            };
            dummyMethod.CilMethodBody.Instructions.CalculateOffsets();
            
            // Step into method.
            _vm.CallStack.Push(dummyMethod);
            
            // Step over first instruction.
            _vm.StepOver();
            
            // We expect to just have moved to the second instruction.
            Assert.Equal(dummyMethod.CilMethodBody.Instructions[1].Offset, _vm.CallStack.Peek().ProgramCounter);
        }

        [Fact]
        public void StepCallShouldContinueInFunction()
        {
            // Prepare dummy methods.
            var foo = new MethodDefinition(
                "Foo", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));
            
            var bar = new MethodDefinition(
                "Bar", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));

            foo.CilMethodBody = new CilMethodBody(foo)
            {
                Instructions =
                {
                    {Call, bar},
                    Ret
                }
            };
            foo.CilMethodBody.Instructions.CalculateOffsets();
            
            bar.CilMethodBody = new CilMethodBody(foo)
            {
                Instructions =
                {
                    {Ldc_I4, 1337},
                    Ret
                }
            };
            bar.CilMethodBody.Instructions.CalculateOffsets();
            
            // Step into method.
            _vm.CallStack.Push(foo);
            
            // Single-step instruction.
            _vm.Invoker = DefaultInvokers.StepIn;
            _vm.Step();
            
            // We expect to have completed "Bar" in its entirety, and moved to the second instruction.
            Assert.Equal(bar, _vm.CallStack.Peek().Method);
            Assert.Equal(bar.CilMethodBody.Instructions[0].Offset, _vm.CallStack.Peek().ProgramCounter);
        }
        
        [Fact]
        public void StepOverCallShouldContinueUntilInstructionAfter()
        {
            // Prepare dummy methods.
            var foo = new MethodDefinition(
                "Foo", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));
            
            var bar = new MethodDefinition(
                "Bar", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));

            foo.CilMethodBody = new CilMethodBody(foo)
            {
                Instructions =
                {
                    {Call, bar},
                    Ret
                }
            };
            foo.CilMethodBody.Instructions.CalculateOffsets();
            
            bar.CilMethodBody = new CilMethodBody(foo)
            {
                Instructions =
                {
                    {Ldc_I4, 1337},
                    Ret
                }
            };
            bar.CilMethodBody.Instructions.CalculateOffsets();
            
            // Step into method.
            _vm.CallStack.Push(foo);
            
            // Step over first instruction.
            _vm.StepOver();
            
            // We expect to have completed "Bar" in its entirety, and moved to the second instruction.
            Assert.Equal(foo, _vm.CallStack.Peek().Method);
            Assert.Equal(foo.CilMethodBody.Instructions[1].Offset, _vm.CallStack.Peek().ProgramCounter);
        }
        
        [Fact]
        public void StepOverBranchShouldContinueAtBranchTarget()
        {
            // Prepare dummy methods.
            var foo = new MethodDefinition(
                "Foo", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(_fixture.MockModule.CorLibTypeFactory.Void));

            var label = new CilInstructionLabel();
            foo.CilMethodBody = new CilMethodBody(foo)
            {
                Instructions =
                {
                    {Br, label},
                    Nop,
                    Nop,
                    Ret
                }
            };
            foo.CilMethodBody.Instructions.CalculateOffsets();
            label.Instruction = foo.CilMethodBody.Instructions[^1];
            
            // Step into method.
            _vm.CallStack.Push(foo);
            
            // Step over first instruction.
            _vm.StepOver();
            
            // We expect to have jumped.
            Assert.Equal(foo, _vm.CallStack.Peek().Method);
            Assert.Equal(label.Offset, _vm.CallStack.Peek().ProgramCounter);
        }

        [Fact]
        public void CallFunctionWithLoop()
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
            var returnValue = _vm.Call(sum, new BitVector[] { arrayAddress });
            Assert.NotNull(returnValue);
            Assert.Equal(Enumerable.Range(100, 10).Sum(), returnValue!.AsSpan().I32);
        }

        [Fact]
        public void CallNestedFunction()
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
            _vm.Invoker = DefaultInvokers.StepIn;

            // Call Foo.
            var returnValue = _vm.Call(foo, Array.Empty<BitVector>());
            Assert.NotNull(returnValue);
            Assert.Equal((3 + 4) * 5, returnValue!.AsSpan().I32);
        }

        [Fact]
        public void CallWithReflection()
        {
            var factory = _fixture.MockModule.CorLibTypeFactory;

            // Look up StringBuilder::Append(string)
            var stringBuilderType = _fixture.MockModule.CorLibTypeFactory.CorLibScope
                .CreateTypeReference("System.Text", "StringBuilder")
                .ImportWith(_fixture.MockModule.DefaultImporter);

            var appendMethod = stringBuilderType
                .CreateMemberReference("Append", MethodSignature.CreateInstance(
                    stringBuilderType.ToTypeSignature(false), factory.String))
                .ImportWith(_fixture.MockModule.DefaultImporter);

            // Prepare dummy method.
            var foo = new MethodDefinition(
                "Foo", 
                MethodAttributes.Static,
                MethodSignature.CreateStatic(
                    factory.Void, 
                    stringBuilderType.ToTypeSignature(false),
                    factory.String));
            
            var fooBody = new CilMethodBody(foo)
            {
                Instructions =
                {
                    Ldarg_0,
                    {Ldstr, "Hello, "},
                    { Callvirt, appendMethod },
                    Ldarg_1,
                    { Callvirt, appendMethod },
                    {Ldstr, ". How are you?"},
                    { Callvirt, appendMethod },
                    Pop,
                    Ret,
                }
            };
            fooBody.Instructions.CalculateOffsets();
            foo.CilMethodBody = fooBody;

            // Set up invoker to reflection invoke StringBuilder::Append(string), and return unknown for everything else. 
            _vm.Invoker = DefaultInvokers.CreateShim()
                .Map((IMethodDescriptor) appendMethod.Resolve()!, DefaultInvokers.ReflectionInvoke)
                .WithFallback(DefaultInvokers.ReturnUnknown);

            // Call it with a real string builder.
            var builder = new StringBuilder();
            var returnValue = _vm.Call(foo, new object[] { builder, "John Doe" });

            // Check result.
            Assert.Null(returnValue);
            Assert.Equal("Hello, John Doe. How are you?", builder.ToString());
        }
    }
}