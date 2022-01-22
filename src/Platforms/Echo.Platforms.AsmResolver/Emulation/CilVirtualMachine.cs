using System;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Memory;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Heap;
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
                Memory.Map(0x7e00_0000, ValueFactory.ClrMockMemory);
                Memory.Map(0x7fe0_0000, CallStack = new CallStack(0x10_0000, ValueFactory));
            }
            else
            {
                Memory.Map(0x0000_0100_0000_0000, Heap = new ManagedObjectHeap(0x01000_0000, ValueFactory));
                Memory.Map(0x0000_7fff_0000_0000, ValueFactory.ClrMockMemory);
                Memory.Map(0x0000_7fff_8000_0000, CallStack = new CallStack(0x100_0000, ValueFactory));
            }

            Dispatcher = new CilDispatcher();
        }

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
        /// Provides the service that is responsible for managing types in the virtual machine.
        /// </summary>
        public ValueFactory ValueFactory
        {
            get;
        }

        /// <summary>
        /// Provides the service that is responsible for mapping executable files in memory.
        /// </summary>
        public PELoader Loader
        {
            get;
        }

        public CilDispatcher Dispatcher
        {
            get;
        }

        public void Run() => Run(CancellationToken.None);
        
        public void Run(CancellationToken cancellationToken)
        {
            var context = new CilExecutionContext(this, cancellationToken);

            do
            {
                Step(context);
                cancellationToken.ThrowIfCancellationRequested();
            } while (CallStack.Count > 0);
        }
        
        public void Step()
        {
            _singleStepContext ??= new CilExecutionContext(this, CancellationToken.None);
            Step(_singleStepContext);
        }

        public void Step(CancellationToken cancellationToken) => Step(new CilExecutionContext(this, cancellationToken));

        private void Step(CilExecutionContext context)
        {
            if (CallStack.Count == 0)
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
            }
        }
    }
}