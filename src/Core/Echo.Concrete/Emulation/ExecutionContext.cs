using System;
using System.Threading;
using Echo.Concrete.Values;
using Echo.Core.Emulation;

namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Provides a context for executing instructions within a virtual machine. 
    /// </summary>
    public class ExecutionContext : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="ExecutionContext"/> class.
        /// </summary>
        /// <param name="programState">The current state of the program.</param>
        /// <param name="cancellationToken">The cancellation token to use for cancelling the execution.</param>
        public ExecutionContext(
            IServiceProvider serviceProvider,
            IProgramState<IConcreteValue> programState, 
            CancellationToken cancellationToken)
        {
            _serviceProvider = serviceProvider;
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

        /// <summary>
        /// Gets or sets a value indicating the execution should terminate.
        /// </summary>
        public bool Exit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the final result of the execution of the program.
        /// </summary>
        public ExecutionResult Result
        {
            get;
        }

        public object GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

        public TService GetService<TService>() => (TService) _serviceProvider.GetService(typeof(TService));
    }
}