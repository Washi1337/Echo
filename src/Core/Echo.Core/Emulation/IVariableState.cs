using System.Collections.Generic;
using Echo.Core.Code;

namespace Echo.Core.Emulation
{
    /// <summary>
    /// Represents a snapshot of all variables and their values at a particular point in time during the execution
    /// of a program.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IVariableState<TValue>
        where TValue : IValue
    {
        /// <summary>
        /// Gets or sets the value currently assigned to a variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        TValue this[IVariable variable]
        {
            get;
            set;
        }

        /// <summary>
        /// Obtains a list of all recorded variables in this snapshot.
        /// </summary>
        /// <returns>The recorded variables.</returns>
        IEnumerable<IVariable> GetAllRecordedVariables();

        /// <summary>
        /// Creates a copy of the snapshot. This also copies all registered values for each variable.
        /// </summary>
        /// <returns>The copied variable state.</returns>
        IVariableState<TValue> Copy();

        /// <summary>
        /// Removes recorded variable.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <returns><see langword="true" /> if the variable is successfully found and removed; otherwise, <see langword="false" />.</returns>
        bool Remove(IVariable variable);
        
        /// <summary>
        /// Removes all variables.
        /// </summary>
        void Clear();
    }
}