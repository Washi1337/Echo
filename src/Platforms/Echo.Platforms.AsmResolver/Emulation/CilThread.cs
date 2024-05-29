using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Represents a single execution thread in a virtualized .NET process.
    /// </summary>
    public class CilThread
    {
        private CilExecutionContext? _singleStepContext;

        internal CilThread(CilVirtualMachine machine, CallStack callStack)
        {
            Machine = machine;
            CallStack = callStack;
            IsAlive = true;
        }

        /// <summary>
        /// Gets the parent machine the thread is running in.
        /// </summary>
        public CilVirtualMachine Machine
        {
            get;
        }

        /// <summary>
        /// Gets the current state of the call stack.
        /// </summary>
        /// <remarks>
        /// The call stack is also addressable from <see cref="Memory"/>.
        /// </remarks>
        public CallStack CallStack
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the thread is alive and present in the parent machine.
        /// </summary>
        public bool IsAlive
        {
            get;
            internal set;
        }

        /// <summary>
        /// Runs the virtual machine until it halts.
        /// </summary>
        public void Run() => Run(CancellationToken.None);

        /// <summary>
        /// Runs the virtual machine until it halts.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used for canceling the emulation.</param>
        public void Run(CancellationToken cancellationToken)
        {
            StepWhile(cancellationToken, context => !context.CurrentFrame.IsRoot);
        }

        /// <summary>
        /// Calls the provided method in the context of the virtual machine.
        /// </summary>
        /// <param name="method">The method to call.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The return value, or <c>null</c> if the provided method does not return a value.</returns>
        /// <remarks>
        /// This method is blocking until the emulation of the call completes.
        /// </remarks>
        public BitVector? Call(IMethodDescriptor method, object[] arguments)
        {
            return Call(method, CancellationToken.None,arguments);
        }
        
        /// <summary>
        /// Calls the provided method in the context of the virtual machine.
        /// </summary>
        /// <param name="method">The method to call.</param>
        /// <param name="cancellationToken">A token that can be used for canceling the emulation.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The return value, or <c>null</c> if the provided method does not return a value.</returns>
        /// <remarks>
        /// This method is blocking until the emulation of the call completes.
        /// </remarks>
        public BitVector? Call(IMethodDescriptor method, CancellationToken cancellationToken, object[] arguments)
        {
            // Short circuit before we do expensive marshalling...
            if (arguments.Length != method.Signature!.GetTotalParameterCount())
                throw new TargetParameterCountException();

            var marshalled = arguments.Select(x => Machine.ObjectMarshaller.ToBitVector(x)).ToArray();
            return Call(method, cancellationToken, marshalled);
        }

        /// <summary>
        /// Calls the provided method in the context of the virtual machine.
        /// </summary>
        /// <param name="method">The method to call.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The return value, or <c>null</c> if the provided method does not return a value.</returns>
        /// <remarks>
        /// This method is blocking until the emulation of the call completes.
        /// </remarks>
        public BitVector? Call(IMethodDescriptor method, BitVector[] arguments)
        {
            return Call(method, CancellationToken.None, arguments);
        }

        /// <summary>
        /// Calls the provided method in the context of the virtual machine.
        /// </summary>
        /// <param name="method">The method to call.</param>
        /// <param name="cancellationToken">A token that can be used for canceling the emulation.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The return value, or <c>null</c> if the provided method does not return a value.</returns>
        /// <remarks>
        /// This method is blocking until the emulation of the call completes or the emulation is canceled.
        /// </remarks>
        public BitVector? Call(IMethodDescriptor method, CancellationToken cancellationToken, BitVector[] arguments)
        {
            if (arguments.Length != method.Signature!.GetTotalParameterCount())
                throw new TargetParameterCountException();

            var pool = Machine.ValueFactory.BitVectorPool;

            // Instantiate any generic types if available.
            var context = GenericContext.FromMethod(method);
            var signature = method.Signature.InstantiateGenericTypes(context);

            // Set up callee frame.
            var frame = new CallFrame(method, Machine.ValueFactory);
            for (int i = 0; i < arguments.Length; i++)
            {
                var slot = Machine.ValueFactory.Marshaller.ToCliValue(arguments[i], signature.ParameterTypes[i]);
                frame.WriteArgument(i, slot.Contents);
                pool.Return(slot.Contents);
            }

            CallStack.Push(frame);

            // Run until we return.
            StepOut(cancellationToken);

            // If void, then we don't have anything else to do.
            if (!signature.ReturnsValue)
                return null;

            // If we produced a return value, return a copy of it to the caller.
            // As the return value may be a rented bit vector, we should copy it to avoid unwanted side-effects.
            var callResult = CallStack.Peek().EvaluationStack.Pop(signature.ReturnType);
            var result = callResult.Clone();
            pool.Return(callResult);

            return result;
        }

        /// <summary>
        /// Continues execution of the virtual machine while the provided predicate returns <c>true</c>.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used for canceling the emulation.</param>
        /// <param name="condition">
        /// A predicate that is evaluated on every step of the emulation, determining whether execution should continue.
        /// </param>
        public void StepWhile(CancellationToken cancellationToken, Predicate<CilExecutionContext> condition)
        {
            var context = new CilExecutionContext(this, cancellationToken);

            do
            {
                Step(context);
                cancellationToken.ThrowIfCancellationRequested();
            } while (condition(context));
        }

        /// <summary>
        /// Performs a single step in the virtual machine. If the current instruction performs a call, the emulation
        /// is treated as a single instruction.
        /// </summary>
        public void StepOver() => StepOver(CancellationToken.None);

        /// <summary>
        /// Performs a single step in the virtual machine. If the current instruction performs a call, the emulation
        /// is treated as a single instruction.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used for canceling the emulation.</param>
        public void StepOver(CancellationToken cancellationToken)
        {
            int stackDepth = CallStack.Count;
            StepWhile(cancellationToken, context => context.Thread.CallStack.Count > stackDepth);
        }

        /// <summary>
        /// Continues execution of the virtual machine until the current call frame is popped from the stack. 
        /// </summary>
        public void StepOut() => StepOut(CancellationToken.None);

        /// <summary>
        /// Continues execution of the virtual machine until the current call frame is popped from the stack. 
        /// </summary>
        /// <param name="cancellationToken">A token that can be used for canceling the emulation.</param>
        public void StepOut(CancellationToken cancellationToken)
        {
            int stackDepth = CallStack.Count;
            StepWhile(cancellationToken, context => context.Thread.CallStack.Count >= stackDepth);
        }

        /// <summary>
        /// Performs a single step in the virtual machine.
        /// </summary>
        public void Step()
        {
            _singleStepContext ??= new CilExecutionContext(this, CancellationToken.None);
            Step(_singleStepContext);
        }

        /// <summary>
        /// Performs a single step in the virtual machine.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used for canceling the emulation.</param>
        public void Step(CancellationToken cancellationToken) => Step(new CilExecutionContext(this, cancellationToken));

        private void Step(CilExecutionContext context)
        {
            if (!IsAlive)
                throw new CilEmulatorException("The thread is not alive.");
            
            if (CallStack.Peek().IsRoot)
                throw new CilEmulatorException("No method is currently being executed.");

            if (CallStack.Peek().IsTrampoline)
            {
                var trampolineFrame = CallStack.Pop();
                if (trampolineFrame.EvaluationStack.Count != 0)
                    CallStack.Peek().EvaluationStack.Push(trampolineFrame.EvaluationStack.Pop());
                return;
            }

            var currentFrame = CallStack.Peek();
            if (currentFrame.Body is not { } body)
                throw new CilEmulatorException("Emulator only supports managed method bodies.");

            // Determine the next instruction to dispatch.
            int pc = currentFrame.ProgramCounter;
            int index = body.Instructions.GetIndexByOffset(pc);

            // Note: This is a loop because instructions can have prefixes. A single step must execute an entire
            // instruction including its prefixes. 
            bool continuing = true;
            while (continuing)
            {
                var instruction = body.Instructions[index];
                if (instruction is null)
                    throw new CilEmulatorException($"Invalid program counter in {currentFrame}.");

                // Continue until we are no longer a prefix instruction.
                continuing = instruction.OpCode.OpCodeType == CilOpCodeType.Prefix;
                index++;
                
                // Are we entering any protected regions?
                UpdateExceptionHandlerStack();

                // Dispatch the instruction.
                var result = Machine.Dispatcher.Dispatch(context, instruction);

                // Did we throw an exception?
                if (!result.IsSuccess)
                {
                    var exceptionObject = result.ExceptionObject;
                    if (exceptionObject.IsNull)
                        throw new CilEmulatorException("A null exception object was thrown.");

                    // If there were any errors thrown after dispatching, it may trigger the execution of one of the
                    // exception handlers in the entire call stack. If no handler is catching it, we communicate it to
                    // the caller of the Echo API itself.
                    if (!UnwindCallStack(ref exceptionObject))
                        throw new EmulatedException(exceptionObject);
                }
            }

            // Reset any prefix-related flags.
            currentFrame.ConstrainedType = null;
        }

        private void UpdateExceptionHandlerStack()
        {
            var currentFrame = CallStack.Peek();

            int pc = currentFrame.ProgramCounter;
            var availableHandlers = currentFrame.ExceptionHandlers;
            var activeHandlers = currentFrame.ExceptionHandlerStack;

            for (int i = 0; i < availableHandlers.Count; i++)
            {
                var handler = availableHandlers[i];
                if (handler.ProtectedRange.Contains(pc) && handler.Enter() && !activeHandlers.Contains(handler))
                    activeHandlers.Push(handler);
            }
        }

        private bool UnwindCallStack(ref ObjectHandle exceptionObject)
        {
            while (!CallStack.Peek().IsRoot)
            {
                var currentFrame = CallStack.Peek();

                // If the exception happened in a .cctor, register it and wrap it in a type initialization error.
                if (currentFrame.Body?.Owner is { IsConstructor: true, IsStatic: true, DeclaringType: {} type })
                    exceptionObject = Machine.TypeManager.RegisterInitializationException(type, exceptionObject);

                var result = currentFrame.ExceptionHandlerStack.RegisterException(exceptionObject);
                if (result.IsSuccess)
                {
                    // We found a handler that needs to be called. Jump to it.
                    currentFrame.ProgramCounter = result.NextOffset;

                    // Push the exception on the stack.
                    currentFrame.EvaluationStack.Clear();
                    currentFrame.EvaluationStack.Push(exceptionObject);

                    return true;
                }

                CallStack.Pop();
            }

            return false;
        }
    }
}