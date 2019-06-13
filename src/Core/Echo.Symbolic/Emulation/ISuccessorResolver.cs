using System.Collections.Generic;
using Echo.Core.Code;
using Echo.Core.Emulation;
using Echo.Symbolic.Values;

namespace Echo.Symbolic.Emulation
{
    public interface ISuccessorResolver
    {
        IEnumerable<IStackState<SymbolicValue>> ResolveNextStates(
            IProgramState<SymbolicValue> programState,
            IInstruction instruction);
    }
}