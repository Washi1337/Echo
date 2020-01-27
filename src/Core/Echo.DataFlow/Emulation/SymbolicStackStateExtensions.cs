using System.Linq;
using Echo.Core.Emulation;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Emulation
{
    /// <summary>
    /// Provides extension methods for stack snapshots that use symbolic values to represent each stack slot.
    /// </summary>
    public static class SymbolicStackStateExtensions
    {
        /// <summary>
        /// Pulls the data sources from the provided stack state, and merges them with each stack slot.
        /// </summary>
        /// <param name="self">The state to modify.</param>
        /// <param name="other">A snapshot of the stack to pull the data sources from.</param>
        /// <returns><c>True</c> if the stack state has changed, <c>false</c> otherwise.</returns>
        /// <exception cref="StackImbalanceException">Occurs when the stack states are of different size.</exception>
        public static bool MergeWith<T>(this IStackState<SymbolicValue<T>> self, IStackState<SymbolicValue<T>> other)
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