using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Concrete.Values.ReferenceType;
using Echo.Core.Code;
using Echo.Core.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Values;
using ExecutionContext = Echo.Concrete.Emulation.ExecutionContext;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a dispatcher based implementation for a virtual machine, capable of emulating a single managed method
    /// body implemented using the CIL instruction set.
    /// </summary>
    public class CilVirtualMachine : IVirtualMachine<CilInstruction>, IServiceProvider, ICilRuntimeEnvironment
    {
        /// <inheritdoc />
        public event EventHandler<ExecutionTerminatedEventArgs> ExecutionTerminated;
     
        private readonly IDictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly IDictionary<string, StringValue> _cachedStrings = new Dictionary<string, StringValue>();

        /// <summary>
        /// Creates a new instance of the <see cref="CilVirtualMachine"/>. 
        /// </summary>
        /// <param name="methodBody">The method body to emulate.</param>
        /// <param name="is32Bit">Indicates whether the virtual machine should run in 32-bit mode or in 64-bit mode.</param>
        public CilVirtualMachine(CilMethodBody methodBody, bool is32Bit)
            : this
            (
                methodBody.Owner.Module,
                new ListInstructionProvider<CilInstruction>(new CilArchitecture(methodBody), methodBody.Instructions),
                is32Bit
            )
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CilVirtualMachine"/>. 
        /// </summary>
        /// <param name="module">The module in which the CIL runs in.</param>
        /// <param name="instructions">The instructions to emulate..</param>
        /// <param name="is32Bit">Indicates whether the virtual machine should run in 32-bit mode or in 64-bit mode.</param>
        public CilVirtualMachine(ModuleDefinition module, IStaticInstructionProvider<CilInstruction> instructions, bool is32Bit)
        {
            Module = module;
            Instructions = instructions;
            Architecture = instructions.Architecture;
            
            Is32Bit = is32Bit;
            Status = VirtualMachineStatus.Idle;
            CurrentState = new CilProgramState();
            Dispatcher = new DefaultCilDispatcher();
            CliMarshaller = new DefaultCliMarshaller(this);
            
            _services[typeof(ICilRuntimeEnvironment)] = this;
        }

        /// <inheritdoc />
        public IInstructionSetArchitecture<CilInstruction> Architecture
        {
            get;
        }

        /// <inheritdoc />
        public bool Is32Bit
        {
            get;
        }

        /// <inheritdoc />
        public ModuleDefinition Module
        {
            get;
        }

        /// <inheritdoc />
        public ICliMarshaller CliMarshaller
        {
            get;
            set;
        }

        /// <inheritdoc />
        public VirtualMachineStatus Status
        {
            get;
            private set;
        }

        /// <inheritdoc />
        public IProgramState<IConcreteValue> CurrentState
        {
            get;
        }

        /// <inheritdoc />
        public IStaticInstructionProvider<CilInstruction> Instructions
        {
            get;
        }

        /// <summary>
        /// Gets or sets the dispatcher used for the execution of instructions.
        /// </summary>
        public IVirtualMachineDispatcher<CilInstruction> Dispatcher
        {
            get;
            set;
        }

        /// <inheritdoc />
        public ExecutionResult Execute(CancellationToken cancellationToken)
        {
            var context = new ExecutionContext(this, CurrentState, cancellationToken);

            try
            {
                Status = VirtualMachineStatus.Running;
                
                // Fetch-execute loop.
                while (!context.Exit)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Fetch.
                    var currentInstruction = Instructions.GetInstructionAtOffset(CurrentState.ProgramCounter);
                    
                    // Execute.
                    var result = Dispatcher.Execute(context, currentInstruction);
                    
                    if (result.IsSuccess)
                    {
                        context.Exit = result.HasTerminated;
                    }
                    else
                    {
                        // Handle exceptions thrown by the instruction. 
                        // TODO: process exception handlers.

                        // Note: We don't throw the user-code exception to conform with spec of the virtual machine
                        // interface.

                        context.Exit = true;
                        context.Result.Exception = result.Exception;
                    }
                }
            }
            finally
            {
                Status = VirtualMachineStatus.Idle;
                OnExecutionTerminated(new ExecutionTerminatedEventArgs(context.Result));
            }

            return context.Result;
        }

        /// <summary>
        /// Invoked when the execution of the virtual machine is terminated.
        /// </summary>
        /// <param name="e">The arguments describing the event.</param>
        protected virtual void OnExecutionTerminated(ExecutionTerminatedEventArgs e) => 
            ExecutionTerminated?.Invoke(this, e);

        object IServiceProvider.GetService(Type serviceType) => 
            _services[serviceType];

        /// <inheritdoc />
        public MemoryPointerValue AllocateMemory(int size, bool initializeWithZeroes)
        {
            var memory = new Memory<byte>(new byte[size]);
            var knownBitMask = new Memory<byte>(new byte[size]);
            if (initializeWithZeroes)
                knownBitMask.Span.Fill(0xFF);
            return new MemoryPointerValue(memory, knownBitMask, Is32Bit);
        }

        /// <inheritdoc />
        public IDotNetArrayValue AllocateArray(TypeSignature elementType, int length)
        {
            if (elementType.IsValueType)
            {
                int size = length * elementType.GetSize(Is32Bit);
                var memory = AllocateMemory(size, true);
                return new ValueTypeArrayValue(elementType, memory);
            }
            
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public StringValue GetStringValue(string value)
        {
            if (!_cachedStrings.TryGetValue(value, out var stringValue))
            {
                var rawMemory = AllocateMemory(value.Length * 2, false);
                var span = new ReadOnlySpan<byte>(Encoding.Unicode.GetBytes(value));
                rawMemory.WriteBytes(0, span);
                stringValue = new StringValue(rawMemory);
                _cachedStrings.Add(value, stringValue);
            }

            return stringValue;
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}