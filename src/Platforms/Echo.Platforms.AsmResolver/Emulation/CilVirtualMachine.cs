using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        /// <summary>
        /// Fires when a new thread was created.
        /// </summary>
        public event EventHandler<CilThread>? ThreadCreated;
        
        /// <summary>
        /// Fires when a thread was destroyed.
        /// </summary>
        public event EventHandler<CilThread>? ThreadDestroyed; 
        
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
            HostObjects = new ObjectMapMemory<object, HostObject>(0x1000_0000, o => new HostObject(o, this));
            ObjectMarshaller = new ObjectMarshaller(this);
            
            if (is32Bit)
            {
                Memory.Map(0x1000_0000, Heap = new ManagedObjectHeap(0x0100_0000, ValueFactory));
                Memory.Map(0x6000_0000, HostObjects);
                Memory.Map(0x7000_0000, StaticFields = new StaticFieldStorage(ValueFactory, 0x0100_0000));
                Memory.Map(0x7100_0000, ValueFactory.ClrMockMemory);
                Memory.Map(0x7f00_0000, _callStackMemory = new CallStackMemory(0x100_0000, ValueFactory));
            }
            else
            {
                Memory.Map(0x0000_0100_0000_0000, Heap = new ManagedObjectHeap(0x01000_0000, ValueFactory));
                Memory.Map(0x0000_7ffd_0000_0000, HostObjects);
                Memory.Map(0x0000_7ffe_0000_0000, StaticFields = new StaticFieldStorage(ValueFactory, 0x1000_0000));
                Memory.Map(0x0000_7ffe_1000_0000, ValueFactory.ClrMockMemory);
                Memory.Map(0x0000_7ffe_8000_0000, _callStackMemory = new CallStackMemory(0x1000_0000, ValueFactory));
            }

            Dispatcher = new CilDispatcher();
            TypeManager = new RuntimeTypeManager(this);
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
        public ObjectMapMemory<object, HostObject> HostObjects
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
        /// Gets the service that is responsible for allocating new objects.
        /// </summary>
        public IObjectAllocator Allocator
        {
            get;
            set;
        } = DefaultAllocators.String.WithFallback(DefaultAllocators.VirtualHeap);

        /// <summary>
        /// Gets the service that is responsible for invoking external functions or methods.
        /// </summary>
        public IMethodInvoker Invoker
        {
            get;
            set;
        } = DefaultInvokers.ReturnUnknown;

        /// <summary>
        /// Gets the service that is responsible for the initialization and management of runtime types residing in
        /// the virtual machine.
        /// </summary>
        public RuntimeTypeManager TypeManager
        {
            get;
        }

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
        /// Gets or sets flags that control the behavior of the virtual machine.
        /// </summary>
        public CilEmulationFlags EmulationFlags
        {
            get;
            set;
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
            ThreadCreated?.Invoke(this, thread);
            return thread;
        }

        /// <summary>
        /// Removes a thread and its stack from the machine. 
        /// </summary>
        /// <param name="thread">The thread to remove.</param>
        /// <remarks>
        /// This does not gracefully terminate a thread. Any code that is still running will remain executing, and may
        /// have unwanted side effects. Therefore, be sure to only call this method only when it is certain that no code
        /// is running. 
        /// </remarks>
        public void DestroyThread(CilThread thread)
        {
            if (thread.Machine != this)
                throw new ArgumentException("Cannot remove a thread from a different machine.");
            if (!thread.IsAlive)
                throw new ArgumentException("Cannot destroy a thread that is already destroyed.");
            
            thread.IsAlive = false;
            _threads.Remove(thread);
            _callStackMemory.Free(thread.CallStack);
            
            ThreadDestroyed?.Invoke(this, thread);
        }
    }
}