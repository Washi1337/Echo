using System;
using Echo.DataFlow.Emulation;

namespace Echo.ControlFlow.Construction.Symbolic
{
    /// <summary>
    /// Represents an object that encodes the transition from one program state to another after an instruction was executed.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction that was executed.</typeparam>
    public readonly struct StateTransition<TInstruction>
    {
        /// <summary>
        /// Creates a new program state transition.
        /// </summary>
        /// <param name="nextState">The new program state.</param>
        /// <param name="edgeType">The type of edge that was taken.</param>
        public StateTransition(SymbolicProgramState<TInstruction> nextState, ControlFlowEdgeType edgeType)
        {
            NextState = nextState ?? throw new ArgumentNullException(nameof(nextState));
            EdgeType = edgeType;
        }
        
        /// <summary>
        /// Gets the new program state after the instruction was executed.
        /// </summary>
        public SymbolicProgramState<TInstruction> NextState
        {
            get;
        }

        /// <summary>
        /// Gets the type of edge that was taken by the instruction.
        /// </summary>
        public ControlFlowEdgeType EdgeType
        {
            get;
        }

        /// <summary>
        /// Gets whether the edge is a real edge (not <see cref="ControlFlowEdgeType.None"/>).
        /// </summary>
        public bool IsRealEdge => EdgeType != ControlFlowEdgeType.None;

        /// <inheritdoc />
        public override string ToString() => NextState is null
            ? "<<unknown>>"
            : $"{NextState.ProgramCounter:X8} ({EdgeType})";
    }
}