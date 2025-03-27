using System.Collections.Generic;
using Echo.Code;

namespace Echo.ControlFlow.Analysis.Liveness;

/// <summary>
/// Provides information about a node that exits the control flow graph.
/// </summary>
/// <param name="Node">The node that exits the control flow graph.</param>
/// <param name="ResultVariables">The variables that the node returns.</param>
/// <typeparam name="TInstruction">The type of instructions stored in the node.</typeparam>
public record struct ExitNodeInfo<TInstruction>(
    ControlFlowNode<TInstruction> Node,
    IReadOnlyList<IVariable> ResultVariables
) where TInstruction : notnull;