using System;
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
    public class CilVirtualMachine : IVirtualMachine<CilInstruction>
    {
        /// <inheritdoc />
        public event EventHandler<ExecutionTerminatedEventArgs> ExecutionTerminated;
        
        private readonly CilMethodBody _methodBody;
        private readonly CilArchitecture _architecture;

        /// <summary>
        /// Creates a new instance of the <see cref="CilVirtualMachine"/>. 
        /// </summary>
        /// <param name="methodBody">The method body to emulate.</param>
        public CilVirtualMachine(CilMethodBody methodBody)
        {
            _methodBody = methodBody ?? throw new ArgumentNullException(nameof(methodBody));
            _architecture = new CilArchitecture(methodBody);
            
            Status = VirtualMachineStatus.Idle;
            CurrentState = new CilProgramState();
            Instructions = new ListInstructionProvider<CilInstruction>(_architecture, methodBody.Instructions);
            Dispatcher = new DefaultCilDispatcher();
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
            var context = new ExecutionContext(CurrentState, cancellationToken);

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
        protected virtual void OnExecutionTerminated(ExecutionTerminatedEventArgs e)
        {
            ExecutionTerminated?.Invoke(this, e);
        }
    }
}