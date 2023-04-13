using System;
using System.IO;
using System.Linq;
using System.Net;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables.Rows;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Stack;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Mocks;
using Xunit;
using static AsmResolver.PE.DotNet.Cil.CilOpCodes;

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

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LeaveMultipleHandlers(bool @throw)
        {
            var factory = _fixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void));

            var tryStart = new CilInstructionLabel();
            var handler1Start = new CilInstructionLabel();
            var handler2Start = new CilInstructionLabel();
            var handler2End = new CilInstructionLabel();

            method.CilMethodBody = new CilMethodBody(method)
            {
                Instructions =
                {
                    Nop,
                    
                    // try {
                    Ldc_I4_1,
                    Stloc_0,
                    {Leave, handler2End},
                    
                    // } catch {
                    Pop,
                    Ldc_I4_2,
                    Stloc_0,
                    {Leave, handler2End},
                    
                    // } finally {
                    Ldloc_0,
                    {Ldc_I4, 100},
                    Add,
                    Stloc_0,
                    Endfinally,
                    // }
                    
                    Ldloc_0,
                    Ret
                },
                ExceptionHandlers =
                {
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Exception,
                        ExceptionType = factory.Object.Type,
                        TryStart = tryStart,
                        TryEnd = handler1Start,
                        HandlerStart = handler1Start,
                        HandlerEnd = handler2Start,
                    },
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Finally,
                        ExceptionType = factory.Object.Type,
                        TryStart = tryStart,
                        TryEnd = handler2Start,
                        HandlerStart = handler2Start,
                        HandlerEnd = handler2End,
                    }
                }
            };
            
            method.CilMethodBody.Instructions.CalculateOffsets();
            tryStart.Instruction = method.CilMethodBody.Instructions[1];
            handler1Start.Instruction = method.CilMethodBody.Instructions[4];
            handler2Start.Instruction = method.CilMethodBody.Instructions[8];
            handler2End.Instruction = method.CilMethodBody.Instructions[13];

            var frame = new CallFrame(method, _vm.ValueFactory);
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[0]);
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[1]);

            ExceptionHandlerResult result;
            
            if (@throw)
            {
                result = frame.ExceptionHandlerStack.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new Exception()));
                Assert.Equal(handler1Start.Offset, result.NextOffset);
            }
            
            int offset = frame.ExceptionHandlerStack.Leave(handler2End.Offset);
            Assert.Equal(handler2Start.Offset, offset);
        
            result = frame.ExceptionHandlerStack.EndFinally();
            Assert.Equal(handler2End.Offset, result.NextOffset);
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