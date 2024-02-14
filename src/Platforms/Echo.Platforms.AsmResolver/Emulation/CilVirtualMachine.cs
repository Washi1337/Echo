using System.Collections.Generic;
using System.Collections.ObjectModel;
using AsmResolver.DotNet;
using Echo.Memory;
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
        private readonly List<CilThread> _threads = new();
        private readonly CallStackMemory _callStackMemory;

        /// <summary>
        /// Creates a new CIL virtual machine.
        /// </summary>
        /// <param name="contextModule">The main module to base the context on.</param>
        /// <param name="is32Bit">Indicates whether the virtual machine runs in 32-bit mode or 64-bit mode.</param>
        public CilVirtualMachine(ModuleDefinition contextModule, bool is32Bit)
        {
            Memory = new VirtualMemory(is32Bit ? uint.MaxValue : long.MaxValue);
            Loader = new PELoader(Memory);
            
            ValueFactory = new ValueFactory(contextModule, is32Bit);
            ObjectMapMemory = new ObjectMapMemory(this, 0x1000_0000);
            ObjectMarshaller = new ObjectMarshaller(this);
            
            if (is32Bit)
            {
                Memory.Map(0x1000_0000, Heap = new ManagedObjectHeap(0x0100_0000, ValueFactory));
                Memory.Map(0x6000_0000, ObjectMapMemory);
                Memory.Map(0x7000_0000, StaticFields = new StaticFieldStorage(ValueFactory, 0x0100_0000));
                Memory.Map(0x7100_0000, ValueFactory.ClrMockMemory);
                Memory.Map(0x7f00_0000, _callStackMemory = new CallStackMemory(0x100_0000, ValueFactory));
            }
            else
            {
                Memory.Map(0x0000_0100_0000_0000, Heap = new ManagedObjectHeap(0x01000_0000, ValueFactory));
                Memory.Map(0x0000_7ffd_0000_0000, ObjectMapMemory);
                Memory.Map(0x0000_7ffe_0000_0000, StaticFields = new StaticFieldStorage(ValueFactory, 0x1000_0000));
                Memory.Map(0x0000_7ffe_1000_0000, ValueFactory.ClrMockMemory);
                Memory.Map(0x0000_7ffe_8000_0000, _callStackMemory = new CallStackMemory(0x1000_0000, ValueFactory));
            }

            Dispatcher = new CilDispatcher();
            Threads = new ReadOnlyCollection<CilThread>(_threads);
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
        /// Gets the memory manager that embeds managed objects into virtual memory.
        /// </summary>
        public ObjectMapMemory ObjectMapMemory
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
        } = DefaultInvokers.ReturnUnknown;

        /// <summary>
        /// Gets or sets the service that is responsible for resolving unknown values on the stack in critical moments.
        /// </summary>
        public IUnknownResolver UnknownResolver
        {
            get; 
            set;
        } = ThrowUnknownResolver.Instance;

        /// <summary>
        /// Gets or sets the service for marshalling managed objects into bitvectors and back.
        /// </summary>
        public IObjectMarshaller ObjectMarshaller
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of threads that are currently active in the machine.
        /// </summary>
        public IReadOnlyList<CilThread> Threads
        {
            get;
        }

        /// <summary>
        /// Creates a new thread in the machine.
        /// </summary>
        /// <param name="stackSize">The amount of memory to allocate for the thread's stack.</param>
        /// <returns>The created thread.</returns>
        public CilThread CreateThread(uint stackSize = 0x0010_0000)
        {
            var stack = _callStackMemory.Allocate(stackSize);
            var thread = new CilThread(this, stack);
            _threads.Add(thread);
            return thread;
        }
    }
}