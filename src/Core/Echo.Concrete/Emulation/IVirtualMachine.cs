using System;
using System.Threading;
using Echo.Concrete.Values;
using Echo.Core.Code;
using Echo.Core.Emulation;

namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Provides an interface to an emulation of a computer system.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to emulate.</typeparam>
    public interface IVirtualMachine<TInstruction>
    {
        /// <summary>
        /// Represents the event that occurs when the execution of the virtual machine has ended.
        /// </summary>
        event EventHandler<ExecutionTerminatedEventArgs> ExecutionTerminated;

        /// <summary>
        /// Gets a value indicating the current status of the virtual machine.
        /// </summary>
        VirtualMachineStatus Status
        {
            get;
        }

        /// <summary>
        /// Gets the current state of the stack and memory of the virtual machine.
        /// </summary>
        IProgramState<IConcreteValue> CurrentState
        {
            get;
        }

        /// <summary>
        /// Gets the instructions in memory to be executed.
        /// </summary>
        IInstructionProvider<TInstruction> Instructions
        {
            get;
        }

        /// <summary>
        /// Executes the instructions until completion.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to use for aborting the execution.</param>
        /// <returns>The produced result.</returns>
        ExecutionResult Execute(CancellationToken cancellationToken);
    }
}