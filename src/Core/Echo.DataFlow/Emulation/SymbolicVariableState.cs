using Echo.Core.Emulation;
using Echo.DataFlow.Values;

namespace Echo.DataFlow.Emulation
{
    public static class SymbolicVariableState
    {
        public static bool MergeWith<T>(this IVariableState<SymbolicValue<T>> self, IVariableState<SymbolicValue<T>> other)
        {
            bool changed = false;
            foreach (var variable in other.GetAllRecordedVariables())
                changed |= self[variable].MergeWith(other[variable]);
            return changed;
        }

    }
}