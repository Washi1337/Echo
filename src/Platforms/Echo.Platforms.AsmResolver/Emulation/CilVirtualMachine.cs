using System;
using System.Reflection;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete;
using Echo.Concrete.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Heap;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Emulation.Runtime;
using Echo.Platforms.AsmResolver.Emulation.Stack;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Represents a machine that executes CIL instructions in a virtual environment. 
    /// </summary>
    public class CilVirtualMachine
    {
        private CilExecutionContext? _singleStepContext;

        /// <summary>
        /// Creates a new CIL virtual machine.
        /// </summary>
        /// <param name="contextModule">The main module to base the context on.</param>
        /// <param name="is32Bit">Indicates whether the virtual machine runs in 32-bit mode or 64-bit mode.</param>
        public CilVirtualMachine(ModuleDefinition contextModule, bool is32Bit)
        {
            Memory = new VirtualMemory(is32Bit ? uint.MaxValue : long.MaxValue);
            ValueFactory = new ValueFactory(contextModule, is32Bit);
            Loader = new PELoader(Memory);

            if (is32Bit)
            {
                Memory.Map(0x1000_0000, Heap = new ManagedObjectHeap(0x0100_0000, ValueFactory));
                Memory.Map(0x7d00_0000, StaticFields = new StaticFieldStorage(ValueFactory, 0x0100_0000));
                Memory.Map(0x7e00_0000, ValueFactory.ClrMockMemory);
                Memory.Map(0x7fe0_0000, CallStack = new CallStack(0x10_0000, ValueFactory));
            }
            else
            {
                Memory.Map(0x0000_0100_0000_0000, Heap = new ManagedObjectHeap(0x01000_0000, ValueFactory));
                Memory.Map(0x0000_7fff_0000_0000, StaticFields = new StaticFieldStorage(ValueFactory, 0x1000_0000));
                Memory.Map(0x0000_7fff_1000_0000, ValueFactory.ClrMockMemory);
                Memory.Map(0x0000_7fff_8000_0000, CallStack = new CallStack(0x100_0000, ValueFactory));
            }

            Dispatcher = new CilDispatcher();
        }

        /// <summary>
        /// Gets a value indicating whether the environment is a 32-bit or 64-bit system.
        /// </summary>
        public bool Is32Bit => ValueFactory.Is32Bit;

        /// <summary>
        /// Gets the main memory interface of the virtual machine.
        /// </summary>
        public VirtualMemory Memory
        {
            get;
        }

        /// <summary>
        /// Gets the heap used for storing managed objects. 
        /// </summary>
        /// <remarks>
        /// The heap is also addressable from <see cref="Memory"/>.
        /// </remarks>
        public ManagedObjectHeap Heap
        {
            get;
        }

        /// <summary>
        /// Gets the memory chunk responsible for storing static fields.
        /// </summary>
        public StaticFieldStorage StaticFields
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
        /// Gets the service that is responsible for managing types in the virtual machine.
        /// </summary>
        public ValueFactory ValueFactory
        {
            get;
        }

        /// <summary>
        /// Gets the main module the emulator is executing instructions for.
        /// </summary>
        public ModuleDefinition ContextModule => ValueFactory.ContextModule;

        /// <summary>
        /// Gets the service that is responsible for mapping executable files in memory.
        /// </summary>
        public PELoader Loader
        {
            get;
        }

        /// <summary>
        /// Gets the service that is responsible for dispatching individual instructions to their respective handlers.
        /// </summary>
        public CilDispatcher Dispatcher
        {
            get;
        }

        /// <summary>
        /// Gets the service that is responsible for invoking external functions or methods.
        /// </summary>
        public IMethodInvoker Invoker
        {
            get;
            set;
        } = ReturnUnknownInvoker.Instance;

        /// <summary>
        /// Gets the service that is responsible for deciding whether a call should be invoked by the <see cref="Invoker"/>,
        /// or whether it should be emulated by the virtual machine.
        /// </summary>
        public IMethodInvocationStrategy InvocationStrategy
        {
            get;
            set;
        } = AlwaysInvokeStrategy.Instance;

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
            StepWhile(cancellationToken, context => !CallStack.Peek().IsRoot);
        }

        /// <summary>
        /// Calls the provided method in the context of the virtual machine.
        /// </summary>
        /// <param name="method">The method to call.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The return value, or <c>null</c> if the provided method does not return a value.</returns>
        /// <remarks>
        /// This method is blocking until the emulation completes.
        /// </remarks>
        public BitVector? Call(IMethodDescriptor method, params BitVector[] arguments)
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
        /// This method is blocking until the emulation completes or the emulation is canceled.
        /// </remarks>
        public BitVector? Call(IMethodDescriptor method, CancellationToken cancellationToken, params BitVector[] arguments)
        {
            if (arguments.Length != method.Signature!.GetTotalParameterCount())
                throw new TargetParameterCountException();

            var pool = ValueFactory.BitVectorPool;
            
            // Instantiate any generic types if available.
            var context = GenericContext.FromMethod(method);
            var signature = method.Signature.InstantiateGenericTypes(context);

            // Set up callee frame.
            var frame = CallStack.Push(method);
            for (int i = 0; i < arguments.Length; i++)
            {
                var slot = ValueFactory.Marshaller.ToCliValue(arguments[i], signature.ParameterTypes[i]);
                frame.WriteArgument(i, slot.Contents);
                pool.Return(slot.Contents);
            }

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
            StepWhile(cancellationToken, context => context.Machine.CallStack.Count > stackDepth);
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
            StepWhile(cancellationToken, context => context.Machine.CallStack.Count >= stackDepth);
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
            if (CallStack.Peek().IsRoot)
                throw new CilEmulatorException("No method is currently being executed.");

            var currentFrame = CallStack.Peek();
            if (currentFrame.Body is not { } body)
                throw new CilEmulatorException("Emulator only supports managed method bodies.");
            
            int pc = currentFrame.ProgramCounter;
            var instruction = body.Instructions.GetByOffset(pc);
            if (instruction is null)
                throw new CilEmulatorException($"Invalid program counter in {currentFrame}.");

            var result = Dispatcher.Dispatch(context, instruction);
            if (!result.IsSuccess)
            {
                // TODO: unwind stack and move to appropriate exception handler if there is any.
                var exceptionPointer = result.ExceptionPointer.AsSpan();
                if (!exceptionPointer.IsFullyKnown)
                {
                    throw new NotImplementedException("Exception handling is not implemented yet (unknown exception type).");
                }

                var type = ValueFactory.ClrMockMemory.MethodTables.GetObject(exceptionPointer.ReadNativeInteger(Is32Bit)); 
                throw new NotImplementedException($"Exception handling is not implemented yet. ({type})");
            }
        }
    }
}