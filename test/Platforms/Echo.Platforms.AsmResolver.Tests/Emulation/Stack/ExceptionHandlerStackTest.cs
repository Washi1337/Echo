using System;
using System.IO;
using System.Net;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Stack
{
    public class ExceptionHandlerStackTest : IClassFixture<MockModuleFixture>
    {
        private readonly MockModuleFixture _fixture;
        private readonly CilVirtualMachine _vm;

        public ExceptionHandlerStackTest(MockModuleFixture fixture)
        {
            _fixture = fixture;
            _vm = new CilVirtualMachine(_fixture.MockModule, false);
        }

        private CallFrame GetMockFrame(string methodName)
        {
            return _vm.CallStack.Push(_fixture.GetTestMethod(methodName));
        }

        private ObjectHandle GetMockException(Type type)
        {
            return _vm.ObjectMarshaller.ToObjectHandle(Activator.CreateInstance(type));
        }
        
        private ObjectHandle GetMockException<T>()
        {
            return _vm.ObjectMarshaller.ToObjectHandle(Activator.CreateInstance<T>());
        }

        [Fact]
        public void ThrowUnhandledExceptionShouldJumpToFinalizer()
        {
            var frame = GetMockFrame(nameof(TestClass.TryFinally));
            
            var stack = frame.ExceptionHandlerStack;          
            stack.Push(frame.ExceptionHandlers[0]); // try-finally
            
            var result = stack.RegisterException(GetMockException<Exception>());
            Assert.True(result.IsSuccess);
            Assert.Equal(frame.ExceptionHandlers[0].Handlers[0].HandlerStart!.Offset, result.NextOffset);
        }

        [Fact]
        public void ThrowHandledExceptionShouldJumpToHandler()
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatchFinally));
            
            var stack = frame.ExceptionHandlerStack;          
            stack.Push(frame.ExceptionHandlers[0]); // try-finally
            stack.Push(frame.ExceptionHandlers[1]); // try-catch
            
            var result = stack.RegisterException(GetMockException<Exception>());
            Assert.True(result.IsSuccess);
            Assert.Equal(frame.ExceptionHandlers[1].Handlers[0].HandlerStart!.Offset, result.NextOffset);
        }

        [Theory]
        [InlineData(typeof(IOException), 0)]
        [InlineData(typeof(WebException), 1)]
        public void ThrowSpecificHandledExceptionShouldJumpToHandler(Type exceptionType, int expectedHandlerIndex)
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatchCatchFinally));
            
            var stack = frame.ExceptionHandlerStack;          
            stack.Push(frame.ExceptionHandlers[0]); // try-finally
            stack.Push(frame.ExceptionHandlers[1]); // try-catch
            
            var result = stack.RegisterException(GetMockException(exceptionType));
            Assert.True(result.IsSuccess);
            Assert.Equal(frame.ExceptionHandlers[1].Handlers[expectedHandlerIndex].HandlerStart!.Offset, result.NextOffset);
        }

        [Fact]
        public void ThrowSpecificUnhandledExceptionShouldJumpToFinalizer()
        {
            var frame = GetMockFrame(nameof(TestClass.TryCatchCatchFinally));
            
            var stack = frame.ExceptionHandlerStack;          
            stack.Push(frame.ExceptionHandlers[0]); // try-finally
            stack.Push(frame.ExceptionHandlers[1]); // try-catch
            
            var result = stack.RegisterException(GetMockException<ArgumentException>());
            Assert.True(result.IsSuccess);
            Assert.Equal(frame.ExceptionHandlers[0].Handlers[0].HandlerStart!.Offset, result.NextOffset);
        }

        [Fact]
        public void EndFinallyAfterUnhandledExceptionShouldExposeException()
        {
            var frame = GetMockFrame(nameof(TestClass.TryFinally));
            
            var stack = frame.ExceptionHandlerStack;          
            stack.Push(frame.ExceptionHandlers[0]); // try-finally
            var exceptionObject = GetMockException<Exception>();
            stack.RegisterException(exceptionObject);

            var result = stack.EndFinally();
            Assert.False(result.IsSuccess);
            Assert.Equal(exceptionObject, result.ExceptionObject);
        }
    }
}