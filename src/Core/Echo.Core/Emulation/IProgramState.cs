using Echo.Core.Values;

namespace Echo.Core.Emulation
{
    /// <summary>
    /// Represents a snapshot of the state of the program in a particular point of execution.
    /// </summary>
    public interface IProgramState<TValue>
        where TValue : IValue
    {
        /// <summary>
        /// Gets the current stack state of the program.
        /// </summary>
        IStackState<TValue> Stack
        {
            get;
        }

        /// <summary>
        /// Gets the current variable state of the program.
        /// </summary>
        IVariableState<TValue> Variables
        {
            get;
        }
    }
}