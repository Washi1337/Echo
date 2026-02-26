using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AsmResolver.DotNet;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Invocation
{
    public class MethodInvokerTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly CilExecutionContext _context;

        public MethodInvokerTest(MockModuleFixture fixture)
        {
            _fixture = fixture;

            var machine = new CilVirtualMachine(fixture.MockModule, false);
            var thread = machine.CreateThread();
            _context = new CilExecutionContext(thread, CancellationToken.None);
            _context.Thread.CallStack.Push(fixture.MockModule.GetOrCreateModuleConstructor());
        }

        [Fact]
        public void ReturnUnknown()
        {
            var invoker = DefaultInvokers.ReturnUnknown;

            var type = _fixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            var method = type.Methods.First(m => m.Name == nameof(TestClass.GetConstantString));

            var result = invoker.Invoke(_context, method, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.StepOver, result.ResultType);
            Assert.NotNull(result.Value);
            Assert.False(result.Value!.AsSpan().IsFullyKnown);
        }

        [Fact]
        public void ReturnUnknownBoolean()
        {
            var invoker = DefaultInvokers.ReturnUnknown;

            var type = _fixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            var method = type.Methods.First(m => m.Name == nameof(TestClass.GetBoolean));

            var result = invoker.Invoke(_context, method, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.StepOver, result.ResultType);
            Assert.NotNull(result.Value);
            Assert.False(result.Value!.AsSpan().IsFullyKnown);

            Assert.Equal(Trilean.Unknown, result.Value.AsSpan()[0]);
            Assert.All(Enumerable.Range(1, result.Value.Count - 1), i => Assert.Equal(Trilean.False, result.Value.AsSpan()[i]));
        }

        [Fact]
        public void ReturnDefault()
        {
            var invoker = DefaultInvokers.ReturnDefault;

            var type = _fixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            var method = type.Methods.First(m => m.Name == nameof(TestClass.GetConstantString));

            var result = invoker.Invoke(_context, method, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.StepOver, result.ResultType);
            Assert.NotNull(result.Value);
            Assert.True(result.Value!.AsSpan().IsFullyKnown);
            Assert.True(result.Value!.AsSpan().IsZero.ToBoolean());
        }

        [Fact]
        public void StepIn()
        {
            var invoker = DefaultInvokers.StepIn;

            var type = _fixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            var method = type.Methods.First(m => m.Name == nameof(TestClass.GetConstantString));

            var result = invoker.Invoke(_context, method, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.StepIn, result.ResultType);
        }

        [Fact]
        public void MethodShim()
        {
            var type = _fixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            var method1 = type.Methods.First(m => m.Name == nameof(TestClass.SingleArgument));
            var method2 = type.Methods.First(m => m.Name == nameof(TestClass.GetIsEvenString));
            var method3 = type.Methods.First(m => m.Name == nameof(TestClass.StaticMethod));

            var invoker1 = new InvokerWrapper(DefaultInvokers.ReturnUnknown);
            var invoker2 = new InvokerWrapper(DefaultInvokers.ReturnUnknown);

            var invoker = DefaultInvokers.CreateShim()
                .Map(method1, invoker1)
                .Map(method2, invoker2);

            var result = invoker.Invoke(_context, method1, new[] { new BitVector(32, false) });
            Assert.Equal(InvocationResultType.StepOver, result.ResultType);
            Assert.Equal(InvocationResultType.StepOver, invoker1.LastInvocationResult.ResultType);
            Assert.Equal(InvocationResultType.Inconclusive, invoker2.LastInvocationResult.ResultType);

            invoker1.LastInvocationResult = InvocationResult.Inconclusive();
            
            result = invoker.Invoke(_context, method2, new[] { new BitVector(32, false) });
            Assert.Equal(InvocationResultType.StepOver, result.ResultType);
            Assert.Equal(InvocationResultType.Inconclusive, invoker1.LastInvocationResult.ResultType);
            Assert.Equal(InvocationResultType.StepOver, invoker2.LastInvocationResult.ResultType);
            
            invoker2.LastInvocationResult = InvocationResult.Inconclusive();
            
            result = invoker.Invoke(_context, method3, new[] { new BitVector(32, false) });
            Assert.Equal(InvocationResultType.Inconclusive, result.ResultType);
            Assert.Equal(InvocationResultType.Inconclusive, invoker1.LastInvocationResult.ResultType);
            Assert.Equal(InvocationResultType.Inconclusive, invoker2.LastInvocationResult.ResultType);
        }

        [Fact]
        public void HandleExternalWith()
        {
            var internalMethod = _fixture.MockModule
                .TopLevelTypes.First(t => t.Name == nameof(TestClass))
                .Methods.First(m => m.Name == nameof(TestClass.NoLocalsNoArguments));

            var externalMethod = ModuleDefinition.FromFile(typeof(Console).Assembly.Location)
                .TopLevelTypes.First(t => t.Name == nameof(Console))
                .Methods.First(m => m.Name == nameof(Console.WriteLine) && m.Signature!.ParameterTypes.Count == 0);
            
            var dummyInvoker = new InvokerWrapper(DefaultInvokers.ReturnUnknown);
            var invoker = DefaultInvokers.HandleExternalWith(dummyInvoker);
                
            var result = invoker.Invoke(_context, internalMethod, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.Inconclusive, result.ResultType);
            Assert.Equal(InvocationResultType.Inconclusive, dummyInvoker.LastInvocationResult.ResultType);
                
            result = invoker.Invoke(_context, externalMethod, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.StepOver, result.ResultType);
            Assert.Equal(InvocationResultType.StepOver, dummyInvoker.LastInvocationResult.ResultType);
        }

        [Fact]
        public void Chain()
        {
            var internalMethod1 = _fixture.MockModule
                .TopLevelTypes.First(t => t.Name == nameof(TestClass))
                .Methods.First(m => m.Name == nameof(TestClass.NoLocalsNoArguments));
            
            var internalMethod2 = _fixture.MockModule
                .TopLevelTypes.First(t => t.Name == nameof(TestClass))
                .Methods.First(m => m.Name == nameof(TestClass.GetConstantString));

            var externalMethod = ModuleDefinition.FromFile(typeof(Console).Assembly.Location)
                .TopLevelTypes.First(t => t.Name == nameof(Console))
                .Methods.First(m => m.Name == nameof(Console.WriteLine) && m.Signature!.ParameterTypes.Count == 0);

            var dummyInvoker = new InvokerWrapper(DefaultInvokers.ReturnUnknown);
            var invoker = DefaultInvokers.CreateShim()
                .Map(internalMethod1, dummyInvoker)
                .WithFallback(DefaultInvokers.ReturnUnknownForExternal)
                .WithFallback(DefaultInvokers.StepIn);
            
            var result = invoker.Invoke(_context, internalMethod1, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.StepOver, result.ResultType);
            Assert.Equal(InvocationResultType.StepOver, dummyInvoker.LastInvocationResult.ResultType);
            
            dummyInvoker.LastInvocationResult = InvocationResult.Inconclusive();
            
            result = invoker.Invoke(_context, internalMethod2, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.StepIn, result.ResultType);
            Assert.Equal(InvocationResultType.Inconclusive, dummyInvoker.LastInvocationResult.ResultType);
            
            result = invoker.Invoke(_context, externalMethod, Array.Empty<BitVector>());
            Assert.Equal(InvocationResultType.StepOver, result.ResultType);
            Assert.Equal(InvocationResultType.Inconclusive, dummyInvoker.LastInvocationResult.ResultType);
        }

        [Fact]
        public void ReflectionInvokeStatic()
        {
            var invoker = DefaultInvokers.ReflectionInvoke;

            var method = _fixture.MockModule
                .TopLevelTypes.First(t => t.Name == nameof(RecordClass))
                .Methods.First(m => m.Name == nameof(RecordClass.StaticMethod));

            var result = invoker.Invoke(_context, method, new[]
            {
                new BitVector(1337),
                new BitVector(1338),
            });
            
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(1337 + 1338, result.Value!.AsSpan().I32);
        }
        
        [Fact]
        public void ReflectionInvokeInstance()
        {
            var invoker = DefaultInvokers.ReflectionInvoke;

            var method = _fixture.MockModule
                .TopLevelTypes.First(t => t.Name == nameof(RecordClass))
                .Methods.First(m => m.Name == nameof(RecordClass.InstanceMethod));

            var instance = new RecordClass(1337, 1338);

            var result = invoker.Invoke(_context, method, new[]
            {
                _context.Machine.ObjectMarshaller.ToBitVector(instance),
                new BitVector(1339)
            });
            
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(1337 + 1338 + 1339, result.Value!.AsSpan().I32);
        }

        [Fact]
        public void DelegateInvokeOnUnknownBackingMethod()
        {
            var testClass = _fixture.MockModule.TopLevelTypes.First(t => t.Name == nameof(TestClass));
            var method = testClass.Methods.First(m => m.Name == nameof(TestClass.ReturnAnyInt));
            var delegateType = testClass.NestedTypes.First(t => t.Name == nameof(TestClass.ReturnAnyIntDelegate));
            var invoke = delegateType.Methods.First(m => m.Name == nameof(TestClass.ReturnAnyIntDelegate.Invoke));

            long instance = _context.Machine.Heap.AllocateObject(delegateType, false);

            _context.Machine.UnknownResolver = new TestDelegateUnknownResolver(method);
            _context.Machine.Invoker = DefaultInvokers.StepIn;
            var result = DefaultInvokers.DelegateShim.Invoke(_context, invoke, [instance]);

            Assert.True(result.IsSuccess);
            Assert.Equal(method, _context.Thread.CallStack.Peek().Method);
        }

        [Fact]
        public void InternString()
        {
            var invoker = StringInvoker.Instance;
            var stringType = _fixture.MockModule.CorLibTypeFactory.String.Type.Resolve()!;
            var internMethod = stringType.Methods.First(m => m.Name == "Intern");

            const string str = "Hello, world!";
            long strAddress = _context.Machine.Heap.AllocateString(str);
            var strArg = _context.Machine.ValueFactory.RentNativeInteger(strAddress);

            var result = invoker.Invoke(_context, internMethod, [strArg]);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.IsFullyKnown);
            
            Assert.True(_context.Machine.Heap.TryGetInternedString(str, out long internedAddress));
            Assert.Equal(internedAddress, result.Value.AsSpan().ReadNativeInteger(_context.Machine.Is32Bit));
        }

        [Fact]
        public void InternSameStringTwiceReturnsSameAddress()
        {
            var invoker = StringInvoker.Instance;
            var stringType = _fixture.MockModule.CorLibTypeFactory.String.Type.Resolve()!;
            var internMethod = stringType.Methods.First(m => m.Name == "Intern");

            const string str = "Hello, world!";
            long strAddress1 = _context.Machine.Heap.AllocateString(str);
            long strAddress2 = _context.Machine.Heap.AllocateString(str);

            var result1 = invoker.Invoke(_context, internMethod, [_context.Machine.ValueFactory.RentNativeInteger(strAddress1)]);
            var result2 = invoker.Invoke(_context, internMethod, [_context.Machine.ValueFactory.RentNativeInteger(strAddress2)]);

            Assert.True(result1.IsSuccess);
            Assert.True(result2.IsSuccess);
            
            Assert.True(result1.Value.IsFullyKnown);
            Assert.True(result2.Value.IsFullyKnown);
            
            Assert.Equal(result1.Value.AsSpan().ReadNativeInteger(_context.Machine.Is32Bit),
                result2.Value.AsSpan().ReadNativeInteger(_context.Machine.Is32Bit));
        }

        [Fact]
        public void IsInternedOnInternedString()
        {
            var invoker = StringInvoker.Instance;
            var stringType = _fixture.MockModule.CorLibTypeFactory.String.Type.Resolve()!;
            var internMethod = stringType.Methods.First(m => m.Name == "Intern");
            var isInternedMethod = stringType.Methods.First(m => m.Name == "IsInterned");

            const string str = "Hello, world!";
            long strAddress = _context.Machine.Heap.AllocateString(str);
            var internResult = invoker.Invoke(_context, internMethod,
                [_context.Machine.ValueFactory.RentNativeInteger(strAddress)]);

            long strAddress2 = _context.Machine.Heap.AllocateString(str);
            var isInternedResult = invoker.Invoke(_context, isInternedMethod,
                [_context.Machine.ValueFactory.RentNativeInteger(strAddress2)]);

            Assert.True(internResult.IsSuccess);
            Assert.True(internResult.Value?.IsFullyKnown);
            
            Assert.True(isInternedResult.IsSuccess);
            Assert.True(isInternedResult.Value?.IsFullyKnown);

            Assert.Equal(internResult.Value!.AsSpan().ReadNativeInteger(_context.Machine.Is32Bit),
                isInternedResult.Value!.AsSpan().ReadNativeInteger(_context.Machine.Is32Bit));
        }

        [Fact]
        public void IsInternedOnNonInternedString()
        {
            var invoker = StringInvoker.Instance;
            var stringType = _fixture.MockModule.CorLibTypeFactory.String.Type.Resolve()!;
            var isInternedMethod = stringType.Methods.First(m => m.Name == "IsInterned");

            const string str = "Never interned";
            long strAddress = _context.Machine.Heap.AllocateString(str);
            var result = invoker.Invoke(_context, isInternedMethod, [_context.Machine.ValueFactory.RentNativeInteger(strAddress)]);

            Assert.True(result.IsSuccess);
            Assert.True(result.Value.IsFullyKnown);
            Assert.Equivalent(result.Value, _context.Machine.ValueFactory.CreateNull());
        }

        private sealed class TestDelegateUnknownResolver(IMethodDescriptor method) : ThrowUnknownResolver
        {
            public override IMethodDescriptor? ResolveDelegateTarget(
                CilExecutionContext context,
                ObjectHandle delegateObject,
                IList<BitVector> arguments)
            {
                return method;
            }
        }
    }
}