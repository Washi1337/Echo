using Echo.Concrete.Values;

namespace Echo.Concrete.Emulation
{
    /// <summary>
    /// Represents an aggregation of results that were produced during the execution of a virtual machine. 
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ExecutionResult"/> class.
        /// </summary>
        /// <param name="returnValue">The produced return value.</param>
        public ExecutionResult(IConcreteValue returnValue)
        {
            ReturnValue = returnValue;
        }

        /// <summary>
        /// Gets the return value that was produced (if any).
        /// </summary>
        /// <remarks>
        /// A value of <c>null</c> indicates no value was produced.
        /// </remarks>
        public IConcreteValue ReturnValue
        {
            get;
            set;
        }
    }
}