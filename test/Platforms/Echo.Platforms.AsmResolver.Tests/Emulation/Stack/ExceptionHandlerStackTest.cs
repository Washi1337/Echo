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
        public void LeaveMultipleHandlersOneByOne(bool @throw)
        {
            var factory = _fixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void));

            var outerTryStart = new CilInstructionLabel();
            var outerHandlerStart = new CilInstructionLabel();
            var outerHandlerEnd = new CilInstructionLabel();
            var innerTryStart = new CilInstructionLabel();
            var innerHandlerStart = new CilInstructionLabel();
            var innerHandlerEnd = new CilInstructionLabel();

            method.CilMethodBody = new CilMethodBody(method)
            {
                Instructions =
                {
                    Nop,
                    
                    // try {
                    //  try {
                    {Leave, innerHandlerEnd},
                    //  } catch {
                    Pop,
                    {Leave, innerHandlerEnd},
                    //  }
                    {Leave, outerHandlerEnd},
                    // } finally {
                    Endfinally,
                    // }
                    
                    Ret
                },
                ExceptionHandlers =
                {
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Finally,
                        TryStart = outerTryStart,
                        TryEnd = outerHandlerStart,
                        HandlerStart = outerHandlerStart,
                        HandlerEnd = outerHandlerEnd,
                    },
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Exception,
                        ExceptionType = factory.Object.Type,
                        TryStart = innerTryStart,
                        TryEnd = innerHandlerStart,
                        HandlerStart = innerHandlerStart,
                        HandlerEnd = innerHandlerEnd,
                    },
                }
            };
            
            outerTryStart.Instruction = method.CilMethodBody.Instructions[1];
            outerHandlerStart.Instruction = method.CilMethodBody.Instructions[5];
            outerHandlerEnd.Instruction = method.CilMethodBody.Instructions[6];
            
            innerTryStart.Instruction = method.CilMethodBody.Instructions[1];
            innerHandlerStart.Instruction = method.CilMethodBody.Instructions[2];
            innerHandlerEnd.Instruction = method.CilMethodBody.Instructions[4];
            
            method.CilMethodBody.Instructions.CalculateOffsets();
            
            var frame = new CallFrame(method, _vm.ValueFactory);
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[0]); // try-finally
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[1]); // try-catch

            if (@throw)
            {
                var result = frame.ExceptionHandlerStack.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new Exception()));
                Assert.Equal(innerHandlerStart.Offset, result.NextOffset);
            }
            
            int nextOffset = frame.ExceptionHandlerStack.Leave(innerHandlerEnd.Offset);
            Assert.Equal(innerHandlerEnd.Offset, nextOffset);

            nextOffset = frame.ExceptionHandlerStack.Leave(outerHandlerEnd.Offset);
            Assert.Equal(outerHandlerStart.Offset, nextOffset);
            
            var finalResult = frame.ExceptionHandlerStack.EndFinally();
            Assert.Equal(outerHandlerEnd.Offset, finalResult.NextOffset);
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LeaveMultipleHandlers(bool @throw)
        {
            var factory = _fixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Int32));

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
        public void NestedFinallyHandlers()
        {
            var factory = _fixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void));

            var outerTryStart = new CilInstructionLabel();
            var outerHandlerStart = new CilInstructionLabel();
            var outerHandlerEnd = new CilInstructionLabel();
            var innerTryStart = new CilInstructionLabel();
            var innerHandlerStart = new CilInstructionLabel();
            var innerHandlerEnd = new CilInstructionLabel();

            method.CilMethodBody = new CilMethodBody(method)
            {
                Instructions =
                {
                    Nop,
                    
                    // try {
                    //  try {
                    {Leave, outerHandlerEnd},
                    //  } finally {
                    Endfinally,
                    //  }
                    // } finally {
                    Endfinally,
                    // }
                    
                    Ret
                },
                ExceptionHandlers =
                {
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Finally,
                        TryStart = outerTryStart,
                        TryEnd = outerHandlerStart,
                        HandlerStart = outerHandlerStart,
                        HandlerEnd = outerHandlerEnd,
                    },
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Finally,
                        TryStart = innerTryStart,
                        TryEnd = innerHandlerStart,
                        HandlerStart = innerHandlerStart,
                        HandlerEnd = innerHandlerEnd,
                    },
                }
            };

            outerTryStart.Instruction = method.CilMethodBody.Instructions[1];
            outerHandlerStart.Instruction = method.CilMethodBody.Instructions[3];
            outerHandlerEnd.Instruction = method.CilMethodBody.Instructions[4];
            innerTryStart.Instruction = method.CilMethodBody.Instructions[1];
            innerHandlerStart.Instruction = method.CilMethodBody.Instructions[2];
            innerHandlerEnd.Instruction = method.CilMethodBody.Instructions[3];
            
            method.CilMethodBody.Instructions.CalculateOffsets();

            var frame = new CallFrame(method, _vm.ValueFactory);
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[0]);
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[1]);

            int nextOffset = frame.ExceptionHandlerStack.Leave(outerHandlerEnd.Offset);
            Assert.Equal(innerHandlerStart.Offset, nextOffset);
            var result = frame.ExceptionHandlerStack.EndFinally();
            Assert.Equal(outerHandlerStart.Offset, result.NextOffset);
            result = frame.ExceptionHandlerStack.EndFinally();
            Assert.Equal(outerHandlerEnd.Offset, result.NextOffset);
        }


        [Fact]
        public void FinallyInCatchHandler()
        {
            var factory = _fixture.MockModule.CorLibTypeFactory;
            var method = new MethodDefinition("dummy", MethodAttributes.Static,
                MethodSignature.CreateStatic(factory.Void));

            var outerTryStart = new CilInstructionLabel();
            var outerHandlerStart = new CilInstructionLabel();
            var outerHandlerEnd = new CilInstructionLabel();
            var innerTryStart = new CilInstructionLabel();
            var innerHandlerStart = new CilInstructionLabel();
            var innerHandlerEnd = new CilInstructionLabel();
            var inner2TryStart = new CilInstructionLabel();
            var inner2HandlerStart = new CilInstructionLabel();
            var inner2HandlerEnd = new CilInstructionLabel();

            method.CilMethodBody = new CilMethodBody(method)
            {
                Instructions =
                {
                    Nop,
                    
                    // try {
                    //  try {
                    {Leave, outerHandlerEnd},
                    //  } catch {
                    Pop,
                    //    try {
                    {Leave, outerHandlerEnd},
                    //    } finally {
                    Endfinally,
                    //    }
                    //  }
                    // } finally {
                    Endfinally,
                    // }
                    
                    Ret
                },
                ExceptionHandlers =
                {
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Finally,
                        TryStart = outerTryStart,
                        TryEnd = outerHandlerStart,
                        HandlerStart = outerHandlerStart,
                        HandlerEnd = outerHandlerEnd,
                    },
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Exception,
                        ExceptionType = factory.Object.Type,
                        TryStart = innerTryStart,
                        TryEnd = innerHandlerStart,
                        HandlerStart = innerHandlerStart,
                        HandlerEnd = innerHandlerEnd,
                    },
                    new CilExceptionHandler
                    {
                        HandlerType = CilExceptionHandlerType.Finally,
                        TryStart = inner2TryStart,
                        TryEnd = inner2HandlerStart,
                        HandlerStart = inner2HandlerStart,
                        HandlerEnd = inner2HandlerEnd,
                    },
                }
            };

            outerTryStart.Instruction = method.CilMethodBody.Instructions[1];
            outerHandlerStart.Instruction = method.CilMethodBody.Instructions[5];
            outerHandlerEnd.Instruction = method.CilMethodBody.Instructions[6];

            innerTryStart.Instruction = method.CilMethodBody.Instructions[1];
            innerHandlerStart.Instruction = method.CilMethodBody.Instructions[2];
            innerHandlerEnd.Instruction = method.CilMethodBody.Instructions[5];

            inner2TryStart.Instruction = method.CilMethodBody.Instructions[3];
            inner2HandlerStart.Instruction = method.CilMethodBody.Instructions[4];
            inner2HandlerEnd.Instruction = method.CilMethodBody.Instructions[5];
            
            method.CilMethodBody.Instructions.CalculateOffsets();

            var frame = new CallFrame(method, _vm.ValueFactory);
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[0]); // outer try-finally
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[1]); // try-catch

            var result = frame.ExceptionHandlerStack.RegisterException(_vm.ObjectMarshaller.ToObjectHandle(new Exception()));
            Assert.Equal(innerHandlerStart.Offset, result.NextOffset);
            
            frame.ExceptionHandlerStack.Push(frame.ExceptionHandlers[2]); // inner try-finally

            int nextOffset = frame.ExceptionHandlerStack.Leave(outerHandlerEnd.Offset);
            Assert.Equal(inner2HandlerStart.Offset, nextOffset);
            result = frame.ExceptionHandlerStack.EndFinally();
            Assert.Equal(outerHandlerStart.Offset, result.NextOffset);
            result = frame.ExceptionHandlerStack.EndFinally();
            Assert.Equal(outerHandlerEnd.Offset, result.NextOffset);
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