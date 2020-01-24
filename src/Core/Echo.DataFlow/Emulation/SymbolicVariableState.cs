using Echo.Core.Emulation;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Emulation
{
    public static class SymbolicVariableState
    {
        public static bool MergeWith(this IVariableState<SymbolicValue> self, IVariableState<SymbolicValue> other)
        {
            bool changed = false;
            foreach (var variable in other.GetAllRecordedVariables())
                changed |= self[variable].MergeWith(other[variable]);
            return changed;
        }

    }
}