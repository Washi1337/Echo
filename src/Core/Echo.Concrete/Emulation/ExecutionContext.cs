using System;
using System.Threading;
using Echo.Concrete.Values;
using Echo.Core.Emulation;

namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Provides a context for executing instructions within a virtual machine. 
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ExecutionContext"/> class.
        /// </summary>
        /// <param name="programState">The current state of the program.</param>
        /// <param name="cancellationToken">The cancellation token to use for cancelling the execution.</param>
        public ExecutionContext(IProgramState<IConcreteValue> programState, CancellationToken cancellationToken)
        {
            ProgramState = programState ?? throw new ArgumentNullException(nameof(programState));
            CancellationToken = cancellationToken;
            Result = new ExecutionResult();
        }
        
        /// <summary>
        /// Gets the current state of the program.
        /// </summary>
        public IProgramState<IConcreteValue> ProgramState
        {
            get;
        }

        /// <summary>
        /// Gets the cancellation token to use for cancelling the execution.
        /// </summary>
        public CancellationToken CancellationToken
        {
            get;
        }

        public bool Exit
        {
            get;
            set;
        }

        public ExecutionResult Result
        {
            get;
        }
    }
}