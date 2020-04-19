using System;
using System.Collections.Generic;
using System.Threading;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using Echo.Concrete.Emulation;
using Echo.Concrete.Emulation.Dispatch;
using Echo.Concrete.Values;
using Echo.Core.Code;
using Echo.Core.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
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
        private readonly CilMethodBody _methodBody;

        /// <summary>
        /// Creates a new instance of the <see cref="CilVirtualMachine"/>. 
        /// </summary>
        /// <param name="methodBody">The method body to emulate.</param>
        /// <param name="is32Bit">Indicates whether the virtual machine should run in 32-bit mode or in 64-bit mode.</param>
        public CilVirtualMachine(CilMethodBody methodBody, bool is32Bit)
        {
            Is32Bit = is32Bit;
            _methodBody = methodBody ?? throw new ArgumentNullException(nameof(methodBody));
            Architecture = new CilArchitecture(methodBody);
            
            Status = VirtualMachineStatus.Idle;
            CurrentState = new CilProgramState();
            Instructions = new ListInstructionProvider<CilInstruction>(Architecture, methodBody.Instructions);
            Dispatcher = new DefaultCilDispatcher();
            
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
        public IInstructionProvider<CilInstruction> Instructions
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
                    
                    // Handle exceptions thrown by the instruction. 
                    if (!result.IsSuccess)
                    {
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
    }
}