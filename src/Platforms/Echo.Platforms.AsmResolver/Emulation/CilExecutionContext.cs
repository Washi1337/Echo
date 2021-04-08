using System;
using System.Threading;
using Echo.Concrete.Emulation;

namespace Echo.Platforms.AsmResolver.Emulation
{
    /// <summary>
    /// Provides a context for executing instructions within a virtual machine. 
    /// </summary>
    public class CilExecutionContext : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="CilExecutionContext"/> class.
        /// </summary>
        /// <param name="serviceProvider">The object providing additional services to the emulator.</param>
        /// <param name="programState">The current state of the program.</param>
        /// <param name="cancellationToken">The cancellation token to use for cancelling the execution.</param>
        public CilExecutionContext(
            IServiceProvider serviceProvider,
            CilProgramState programState, 
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
        public CilProgramState ProgramState
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

        /// <inheritdoc />
        public object GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>The service object.</returns>
        public TService GetService<TService>() => (TService) _serviceProvider.GetService(typeof(TService));
    }
}