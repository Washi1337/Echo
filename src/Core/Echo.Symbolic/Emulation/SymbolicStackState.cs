using System.Linq;
using Echo.Core.Emulation;
using Echo.Symbolic.Values;

namespace Echo.Symbolic.Emulation
{
    /// <summary>
    /// Represents a stack that contains only symbolic values, and provides functionality to merge with other
    /// stack states to merge data sources of each individual stack slot.
    /// </summary>
    public static class SymbolicStackState
    {
        /// <summary>
        /// Pulls the data sources from the provided stack state, and merges them with each stack slot.
        /// </summary>
        /// <param name="self">The state to modify.</param>
        /// <param name="other">The state of the stack to pull the data sources from.</param>
        /// <returns><c>True</c> if the stack state has changed, <c>false</c> otherwise.</returns>
        /// <exception cref="StackImbalanceException">Occurs when the stack states are of different size.</exception>
        public static bool MergeWith(this IStackState<SymbolicValue> self, IStackState<SymbolicValue> other)
        {
            if (self.Size != other.Size)
                throw new StackImbalanceException();

            var zipped = self
                .GetAllStackSlots()
                .Zip(other.GetAllStackSlots(), (a, b) => (a, b));
            
            bool changed = false;
            foreach (var (a, b) in zipped)
                changed |= a.MergeWith(b);
            
            return changed;
        }
    }
}