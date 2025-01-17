using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Tests.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;
using MethodAttributes = AsmResolver.PE.DotNet.Metadata.Tables.MethodAttributes;
using MethodDefinition = AsmResolver.DotNet.MethodDefinition;
using TestClass = Mocks.TestClass;

namespace Echo.Platforms.AsmResolver.Tests.Emulation
{
    public class CilVirtualMachineTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly CilVirtualMachine _vm;
        private readonly CilThread _mainThread;

        public CilVirtualMachineTest(MockModuleFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _fixture = fixture;
            _testOutputHelper = testOutputHelper;
            _vm = new CilVirtualMachine(fixture.MockModule, false);
            _mainThread = _vm.CreateThread();
        }

        [Fact]
        public void CreateSingleThread()
        {
            Assert.Contains(_mainThread, _vm.Threads);
        }

        [Fact]
        public void CreateSecondaryThread()
        {
            var thread = _vm.CreateThread();
            Assert.Contains(_mainThread, _vm.Threads);
            Assert.False(thread.CallStack.AddressRange.Contains(_mainThread.CallStack.AddressRange.Start));
            Assert.False(thread.CallStack.AddressRange.Contains(_mainThread.CallStack.AddressRange.End - 1));
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
            _mainThread.CallStack.Push(dummyMethod);

            // Execute all nops.
            for (int i = 0; i < 100; i++)
                _mainThread.Step();

            // Check if we're still in the dummy method.
            Assert.Equal(2, _mainThread.CallStack.Count);
            
            // Execute return.
            _mainThread.Step();
            
            // Check if we exited.
            Assert.True(Assert.Single(_mainThread.CallStack).IsRoot);
        }

        [Fact]
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
            _mainThread.CallStack.Push(dummyMethod);

            _mainThread.Run();
            
            // Check if we exited.
            Assert.True(Assert.Single(_mainThread.CallStack).IsRoot);
        }

        [Fact]
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
            _mainThread.CallStack.Push(dummyMethod);

            var tokenSource = new CancellationTokenSource();
            
            int dispatchCounter = 0;
            _vm.Dispatcher.BeforeInstructionDispatch += (_, _) =>
            {
                dispatchCounter++;
                if (dispatchCounter == 300)
                    tokenSource.Cancel();
            };

            Assert.Throws<OperationCanceledException>(() => _mainThread.Run(tokenSource.Token));;
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
            var frame = _mainThread.CallStack.Push(dummyMethod);

            for (int i = 0; i < 5; i++)
                _mainThread.Step();

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
            _mainThread.CallStack.Push(dummyMethod);
            
            // Step over first instruction.
            _mainThread.StepOver();
            
            // We expect to just have moved to the second instruction.
            Assert.Equal(dummyMethod.CilMethodBody.Instructions[1].Offset, _mainThread.CallStack.Peek().ProgramCounter);
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
            _mainThread.CallStack.Push(foo);
            
            // Single-step instruction.
            _vm.Invoker = DefaultInvokers.StepIn;
            _mainThread.Step();
            
            // We expect to have completed "Bar" in its entirety, and moved to the second instruction.
            Assert.Equal(bar, _mainThread.CallStack.Peek().Method);
            Assert.Equal(bar.CilMethodBody.Instructions[0].Offset, _mainThread.CallStack.Peek().ProgramCounter);
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
            _mainThread.CallStack.Push(foo);
            
            // Step over first instruction.
            _mainThread.StepOver();
            
            // We expect to have completed "Bar" in its entirety, and moved to the second instruction.
            Assert.Equal(foo, _mainThread.CallStack.Peek().Method);
            Assert.Equal(foo.CilMethodBody.Instructions[1].Offset, _mainThread.CallStack.Peek().ProgramCounter);
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
            _mainThread.CallStack.Push(foo);
            
            // Step over first instruction.
            _mainThread.StepOver();
            
            // We expect to have jumped.
            Assert.Equal(foo, _mainThread.CallStack.Peek().Method);
            Assert.Equal(label.Offset, _mainThread.CallStack.Peek().ProgramCounter);
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
            var returnValue = _mainThread.Call(sum, new BitVector[] { arrayAddress });
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
            var returnValue = _mainThread.Call(foo, Array.Empty<BitVector>());
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
            var returnValue = _mainThread.Call(foo, new object[] { builder, "John Doe" });

            // Check result.
            Assert.Null(returnValue);
            Assert.Equal("Hello, John Doe. How are you?", builder.ToString());
        }

        [Fact]
        public void CallTryFinallyNoException()
        {
            var method = _fixture.GetTestMethod(nameof(TestClass.TryFinally));

            var result = _mainThread.Call(method, new object[] { false });
            Assert.NotNull(result);
            Assert.Equal(101, result!.AsSpan().I32);
        }
        
        [Fact]
        public void CallTryFinallyException()
        {
            var method = _fixture.GetTestMethod(nameof(TestClass.TryFinally));

            var result = Assert.Throws<EmulatedException>(() => _mainThread.Call(method, new object[] {true}));
            Assert.Equal("System.Exception", result.ExceptionObject.GetObjectType().FullName);
        }
        
        [Theory]
        [InlineData( nameof(TestClass.TryCatch), false)]
        [InlineData(nameof(TestClass.TryCatch), true)]
        [InlineData( nameof(TestClass.TryCatchFinally), false)]
        [InlineData(nameof(TestClass.TryCatchFinally), true)]
        [InlineData(nameof(TestClass.TryCatchCatch), 0)]
        [InlineData(nameof(TestClass.TryCatchCatch), 1)]
        [InlineData(nameof(TestClass.TryCatchCatch), 3)]
        [InlineData(nameof(TestClass.TryCatchSpecificAndGeneral), 0)]
        [InlineData(nameof(TestClass.TryCatchSpecificAndGeneral), 1)]
        [InlineData(nameof(TestClass.TryCatchSpecificAndGeneral), 3)]
        [InlineData(nameof(TestClass.TryCatchFilters), 0)]
        [InlineData(nameof(TestClass.TryCatchFilters), 1)]
        [InlineData(nameof(TestClass.TryCatchFilters), 4)]
        [InlineData( nameof(TestClass.CatchExceptionInChildMethod), false)]
        [InlineData(nameof(TestClass.CatchExceptionInChildMethod), true)]
        public void CallAndThrowHandledException(string methodName, object parameter)
        {
            _vm.Invoker = DefaultInvokers.ReturnUnknownForNative.WithFallback(DefaultInvokers.StepIn);
            
            var method = _fixture.GetTestMethod(methodName);
            
            var reflectionMethod = typeof(TestClass).GetMethod(methodName);
            int expectedResult = (int) reflectionMethod!.Invoke(null, new[] {parameter})!;

            var result = _mainThread.Call(method, new[] {parameter});
            
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result!.AsSpan().I32);
        }

        [Theory]
        [InlineData( nameof(TestClass.TryCatchCatch), 2)]
        [InlineData( nameof(TestClass.TryCatchSpecificAndGeneral), 2)]
        public void CallAndThrowUnhandledException(string methodName, object parameter)
        {
            _vm.Invoker = DefaultInvokers.ReturnUnknownForExternal.WithFallback(DefaultInvokers.StepIn);
            
            var method = _fixture.GetTestMethod(methodName);
            
            var reflectionMethod = typeof(TestClass).GetMethod(methodName);
            
            Exception? expectedException;
            try
            {
                reflectionMethod!.Invoke(null, new[] {parameter});
                throw new XunitException(
                    $"Test method {methodName} does not throw an exception with argument {parameter}.");
            }
            catch (TargetInvocationException ex)
            {
                expectedException = ex.InnerException!;
            }

            var result = Assert.Throws<EmulatedException>(() => _mainThread.Call(method, new[] {parameter}));
            Assert.Equal(expectedException.GetType().FullName, result.ExceptionObject.GetObjectType().FullName);
        }

        [Fact]
        public void SteppingWithNestedInitializers()
        {
            // Look up metadata.
            var method = _fixture
                .MockModule.LookupMember<TypeDefinition>(typeof(ClassWithNestedInitializer.Class1).MetadataToken)
                .Methods.First(m => m.Name == nameof(ClassWithNestedInitializer.Class1.Method));

            // CAll method.
            _vm.Invoker = DefaultInvokers.StepIn;
            var result = _mainThread.Call(method, Array.Empty<BitVector>());
            
            // Verify.
            Assert.NotNull(result);
            Assert.Equal(1337, result.AsSpan().I32);
        }

        [Fact]
        public void StaticFieldWithInitialValueUpdate()
        {
            var type = _fixture.MockModule.LookupMember<TypeDefinition>(typeof(ClassWithInitializer).MetadataToken);
            var increment = type.Methods.First(m => m.Name == nameof(ClassWithInitializer.IncrementCounter));
            var counter = type.Fields.First(f => f.Name == nameof(ClassWithInitializer.Counter));
            
            // Verify uninitialized value.
            Assert.Equal(0, _vm.StaticFields.GetFieldSpan(counter).I32);
            
            // Call and verify value.
            _mainThread.Call(increment, Array.Empty<BitVector>());
            Assert.Equal(1337 + 1, _vm.StaticFields.GetFieldSpan(counter).I32);
            _mainThread.Call(increment, Array.Empty<BitVector>());
            Assert.Equal(1337 + 2, _vm.StaticFields.GetFieldSpan(counter).I32);
            _mainThread.Call(increment, Array.Empty<BitVector>());
            Assert.Equal(1337 + 3, _vm.StaticFields.GetFieldSpan(counter).I32);
        }

        [Fact]
        public void StepPrefixedInstructionsShouldStepOverAllInstructions()
        {
            var factory = _fixture.MockModule.CorLibTypeFactory;
            var toString = factory.Object.Type
                .CreateMemberReference("ToString", MethodSignature.CreateInstance(factory.String));
            
            var dummyMethod = new MethodDefinition(
                "DummyMethod",
                MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.String, 1,
                    new GenericParameterSignature(GenericParameterType.Method, 0))
            );
            dummyMethod.GenericParameters.Add(new GenericParameter("T"));
            dummyMethod.CilMethodBody = new CilMethodBody(dummyMethod)
            {
                Instructions =
                {
                    {Ldarga_S, dummyMethod.Parameters[0]},
                    {Constrained, new GenericParameterSignature(GenericParameterType.Method, 0).ToTypeDefOrRef()},
                    {Callvirt, toString},
                    Ret
                }
            };
            dummyMethod.CilMethodBody.Instructions.CalculateOffsets();

            var frame = _mainThread.CallStack.Push(dummyMethod.MakeGenericInstanceMethod(factory.Int32));
            frame.WriteArgument(0, new BitVector(1337));

            var instructions = dummyMethod.CilMethodBody.Instructions;
            
            Assert.Equal(instructions[0].Offset, frame.ProgramCounter);
            _mainThread.Step();
            Assert.Equal(instructions[1].Offset, frame.ProgramCounter);
            _mainThread.Step();
            Assert.Equal(instructions[3].Offset, frame.ProgramCounter);
        }
        
        [Fact]
        public void CallDelegate()
        {
            var method = _fixture.MockModule
                    .LookupMember<TypeDefinition>(typeof(TestClass).MetadataToken)
                    .Methods.First(m => m.Name == nameof(TestClass.TestDelegateCall));

            _vm.Invoker = DefaultInvokers.DelegateShim.WithFallback(DefaultInvokers.StepIn);
            _mainThread.CallStack.Push(method);

            var instructions = method.CilMethodBody!.Instructions;

            var callDelegateOffset = instructions.First(instruction => instruction.OpCode.Code == CilCode.Callvirt).Offset;

            _mainThread.StepWhile(CancellationToken.None, context => context.CurrentFrame.ProgramCounter != callDelegateOffset);
            _mainThread.Step(); // call delegate::invoke
            // callstack:
            // (0) root -> (1) TestClass::TestDelegateCall -> (2) ReturnAnyIntDelegate::Invoke -> (3) TestClass::ReturnAnyInt
            Assert.Equal(4, _mainThread.CallStack.Count);

            _mainThread.StepOut();
            // callstack:
            // (0) root -> (1) TestClass::TestDelegateCall -> (2) ReturnAnyIntDelegate::Invoke
            // evaluation stack:
            // (0) i32: 5
            Assert.Equal(3, _mainThread.CallStack.Count);
            Assert.Single(_mainThread.CallStack.Peek().EvaluationStack);
            Assert.Equal(5, _mainThread.CallStack.Peek().EvaluationStack.Peek().Contents.AsSpan().I32);

            _mainThread.StepOut();
            // callstack:
            // (0) root -> (1) TestClass::TestDelegateCall
            // evaluation stack:
            // (0) i32: 5
            Assert.Equal(2, _mainThread.CallStack.Count);
            Assert.Single(_mainThread.CallStack.Peek().EvaluationStack);
            Assert.Equal(5, _mainThread.CallStack.Peek().EvaluationStack.Peek().Contents.AsSpan().I32);

            _mainThread.StepOut();
            // callstack:
            // (0) root
            // evaluation stack:
            // (0) i32: 5
            Assert.Single(_mainThread.CallStack);
            Assert.Single(_mainThread.CallStack.Peek().EvaluationStack);
            Assert.Equal(5, _mainThread.CallStack.Peek().EvaluationStack.Peek().Contents.AsSpan().I32);
        }
    }
}