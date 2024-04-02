using System;
using System.IO;
using System.Linq;
using System.Net;
using AsmResolver.DotNet.Code.Cil;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Stack
{
    public class ExceptionHandlerFrameTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly CilVirtualMachine _vm;
        private readonly CilThread _mainThread;

        public ExceptionHandlerFrameTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            _vm = new CilVirtualMachine(_fixture.MockModule, false);
            _mainThread = _vm.CreateThread();
        }

        private CallFrame GetMockFrame(string methodName)
        {
            return _mainThread.CallStack.Push(_fixture.GetTestMethod(methodName));
        }

        [Fact]
        public void LeaveShouldJumpToOffsetIfNoFinalizerPresent()
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatch)).ExceptionHandlers.First(x => !x.HasFinalizer);

            int? offset = frame.Leave(1337);
            Assert.Equal(1337, offset);
        }

        [Fact]
        public void LeaveShouldJumpToFinalizerIfPresent()
        {
            var frame = GetMockFrame(nameof(TestClass.TryFinally)).ExceptionHandlers.First(x => x.HasFinalizer);

            int? offset = frame.Leave(1337);
            Assert.Equal(
                frame.Handlers.First(x => x.HandlerType == CilExceptionHandlerType.Finally).HandlerStart!.Offset,
                offset);
            Assert.Equal(1337, frame.NextOffset);
        }

        [Fact]
        public void EndFinallyShouldJumpToLeaveOffsetIfPresent()
        {
            var frame = GetMockFrame(nameof(TestClass.TryFinally)).ExceptionHandlers.First(x => x.HasFinalizer);
            frame.Leave(1337);
            Assert.Equal(1337, frame.EndFinally());
        }

        [Fact]
        public void EndFinallyShouldReturnNullIfNoLeave()
        {
            var frame = GetMockFrame(nameof(TestClass.TryFinally)).ExceptionHandlers.First(x => x.HasFinalizer);
            Assert.False(frame.EndFinally().HasValue);
        }

        [Theory]
        [InlineData(typeof(IOException), 0)]
        [InlineData(typeof(WebException), 1)]
        [InlineData(typeof(ArgumentException), null)]
        public void RegisterExceptionShouldJumpToFirstCompatibleException(Type exceptionType, int? expectedIndex)
        {
            var exception = _vm.ObjectMarshaller.ToObjectHandle(Activator.CreateInstance(exceptionType));
            var frame = GetMockFrame(nameof(TestClass.TryCatchCatch)).ExceptionHandlers[0];

            Assert.Equal(new[]
            {
                "System.IO.IOException",
                "System.Net.WebException"
            }, frame.Handlers.Select(x => x.ExceptionType!.FullName));

            int? offset = frame.RegisterException(exception);
            if (expectedIndex.HasValue)
                Assert.Same(frame.Handlers[expectedIndex.Value], frame.CurrentHandler);
            else
                Assert.False(offset.HasValue);
        }

        [Fact]
        public void RegisterExceptionInCatchClauseShouldReturnNull()
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatchSpecificAndGeneral)).ExceptionHandlers[0];
            
            Assert.Equal(new[]
            {
                "System.IO.EndOfStreamException",
                "System.IO.IOException"
            }, frame.Handlers.Select(x=>x.ExceptionType!.FullName));

            Assert.True(frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new EndOfStreamException())).HasValue);
            Assert.False(frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new IOException())).HasValue);
        }

        [Fact]
        public void EndFilterTrueShouldJumpToHandler()
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatchFilters)).ExceptionHandlers[0];
            frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new IOException()));
            
            int? offset = frame.EndFilter(true);
            Assert.Equal(
                frame.Handlers.First(x=>x.HandlerType == CilExceptionHandlerType.Filter).HandlerStart!.Offset,
                offset);
        }

        [Fact]
        public void EndFilterFalseShouldJumpToNextCompatibleHandler()
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatchFilters)).ExceptionHandlers[0];
            frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new IOException()));
            
            int? offset = frame.EndFilter(false);
            Assert.Equal(
                frame.Handlers
                    .Where(x => x.HandlerType == CilExceptionHandlerType.Filter)
                    .ElementAt(1).FilterStart!.Offset,
                offset);
        }

        [Fact]
        public void RegisterExceptionDuringFilteringShouldMoveToNextHandler()
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatchFilters)).ExceptionHandlers[0];
            
            Assert.True(frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new IOException())).HasValue);
            int? offset = frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new NullReferenceException()));
            Assert.Equal(
                frame.Handlers
                    .Where(x => x.HandlerType == CilExceptionHandlerType.Filter)
                    .ElementAt(1).FilterStart!.Offset,
                offset);
        }

        [Fact]
        public void RegisterExceptionDuringFilteringShouldReturnNullIfNoMoreCompatibleHandlers()
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatchFilters)).ExceptionHandlers[0];
            
            Assert.True(frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new IOException())).HasValue);
            Assert.True(frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new NullReferenceException())).HasValue);
            int? offset = frame.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new NullReferenceException()));
            Assert.False(offset.HasValue);
        }
    }
}