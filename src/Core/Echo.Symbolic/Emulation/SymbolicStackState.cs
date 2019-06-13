using Echo.Core.Emulation;
using Echo.Symbolic.Values;

namespace Echo.Symbolic.Emulation
{
    /// <summary>
    /// Represents a stack that contains only symbolic values, and provides functionality to merge with other
    /// stack states to merge data sources of each individual stack slot.
    /// </summary>
    public class SymbolicStackState : StackState<SymbolicValue>
    {
        /// <summary>
        /// Pulls the data sources from the provided stack state, and merges them with each stack slot.
        /// </summary>
        /// <param name="other">The state of the stack to pull the data sources from.</param>
        /// <returns><c>True</c> if the stack state has changed, <c>false</c> otherwise.</returns>
        /// <exception cref="StackImbalanceException">Occurs when the stack states are of different size.</exception>
        public bool MergeWith(IStackState<SymbolicValue> other)
        {
            if (Size != other.Size)
                throw new StackImbalanceException();

            bool changed = false;
            int index = Size - 1;
            foreach (var item in other.GetAllStackSlots())
            {
                changed |= Items[index].MergeWith(item);
                index--;
            }

            return changed;
        }
    }
}