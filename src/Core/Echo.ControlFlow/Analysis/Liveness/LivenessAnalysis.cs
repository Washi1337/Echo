using System;
using System.Collections.Generic;
using Echo.Code;

namespace Echo.ControlFlow.Analysis.Liveness;

/// <summary>
/// Provides factory methods for determining liveness information of a function.
/// </summary>
public static class LivenessAnalysis
{
    /// <summary>
    /// Analyzes the liveness of variables in the provided control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to analyze.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    /// <returns>The liveness analysis.</returns>
    public static LivenessAnalysis<TInstruction> FromFlowGraph<TInstruction>(ControlFlowGraph<TInstruction> cfg)
        where TInstruction : notnull
    {
        return FromFlowGraph(cfg, Array.Empty<ExitNodeInfo<TInstruction>>());
    }

    /// <summary>
    /// Analyzes the liveness of variables in the provided control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to analyze.</param>
    /// <param name="resultVariables">The set of variables that will be considered alive after returning.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    /// <returns>The liveness analysis.</returns>
    public static LivenessAnalysis<TInstruction> FromFlowGraph<TInstruction>(ControlFlowGraph<TInstruction> cfg, IVariable[] resultVariables)
        where TInstruction : notnull
    {
        // Find exit nodes based on returning/terminating instructions.
        var knownExitNodes = new List<ExitNodeInfo<TInstruction>>();
        foreach (var node in cfg.Nodes)
        {
            if (node.Contents.Footer is { } instruction && (cfg.Architecture.GetFlowControl(instruction) & InstructionFlowControl.IsTerminator) != 0)
                knownExitNodes.Add(new ExitNodeInfo<TInstruction>(node, resultVariables));
        }

        return FromFlowGraph(cfg, knownExitNodes.ToArray());
    }

    /// <summary>
    /// Analyzes the liveness of variables in the provided control flow graph.
    /// </summary>
    /// <param name="cfg">The control flow graph to analyze.</param>
    /// <param name="knownExitNodes">A collection of nodes and their result variables that will be considered alive after exiting.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    /// <returns>The liveness analysis.</returns>
    public static LivenessAnalysis<TInstruction> FromFlowGraph<TInstruction>(ControlFlowGraph<TInstruction> cfg, ExitNodeInfo<TInstruction>[] knownExitNodes)
        where TInstruction : notnull
    {
        return LivenessAnalyzer<TInstruction>.Analyze(cfg, knownExitNodes);
    }
}

/// <summary>
/// Provides liveness information for instructions in a control flow graph.
/// </summary>
/// <param name="controlFlowGraph">The control flow graph the liveness was determined for.</param>
/// <param name="variables">The variables involved.</param>
/// <param name="data">The liveness data.</param>
/// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
public class LivenessAnalysis<TInstruction>(
    ControlFlowGraph<TInstruction> controlFlowGraph,
    ISet<IVariable> variables,
    IReadOnlyDictionary<TInstruction, LivenessData> data
) where TInstruction : notnull
{
    private readonly IReadOnlyDictionary<TInstruction, LivenessData> _data = data;

    /// <summary>
    /// Gets the control flow graph the liveness was determined for.
    /// </summary>
    public ControlFlowGraph<TInstruction> ControlFlowGraph { get; } = controlFlowGraph;

    /// <summary>
    /// Gets the variables involved.
    /// </summary>
    public ISet<IVariable> Variables { get; } = variables;

    /// <summary>
    /// Gets the liveness data for the provided instruction.
    /// </summary>
    /// <param name="instruction">The instruction.</param>
    public LivenessData this[TInstruction instruction]
    {
        get
        {
            if (!_data.TryGetValue(instruction, out var data))
                data = LivenessData.Empty;
            return data;
        }
    }
}