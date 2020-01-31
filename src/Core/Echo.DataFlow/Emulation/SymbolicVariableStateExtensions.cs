using Echo.Core.Emulation;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Emulation
{
    /// <summary>
    /// Provides extension methods to variable states that use symbolic values to represent the contents of each variable.
    /// </summary>
    public static class SymbolicVariableStateExtensions
    {
        /// <summary>
        /// Pulls the data sources from the provided variable state, and merges them with each variable slot.
        /// </summary>
        /// <param name="self">The state to modify.</param>
        /// <param name="other">A snapshot of variable states to pull the data sources from.</param>
        /// <returns><c>True</c> if the variables state has changed, <c>false</c> otherwise.</returns>
        public static bool MergeWith<T>(this IVariableState<SymbolicValue<T>> self, IVariableState<SymbolicValue<T>> other)
        {
            bool changed = false;
            foreach (var variable in other.GetAllRecordedVariables())
                changed |= self[variable].MergeWith(other[variable]);
            return changed;
        }

    }
}