using System.Collections.Generic;
using System.Linq;
using Echo.Code;
using Echo.Graphing.Analysis.Traversal;

namespace Echo.ControlFlow.Analysis.Liveness;

internal sealed class LivenessAnalyzer<TInstruction>
    where TInstruction : notnull
{
    private readonly ExitNodeInfo<TInstruction>[] _knownExitNodes;
    private readonly ControlFlowGraph<TInstruction> _cfg;
    private readonly HashSet<IVariable> _variables;

    private readonly Dictionary<TInstruction, LivenessData> _instructionLiveness;
    private readonly Dictionary<ControlFlowNode<TInstruction>, LivenessData> _nodeLiveness;

    private LivenessAnalyzer(ControlFlowGraph<TInstruction> cfg, ExitNodeInfo<TInstruction>[] knownExitNodes)
    {
        _knownExitNodes = knownExitNodes;
        _cfg = cfg;
        _variables = new HashSet<IVariable>(knownExitNodes.SelectMany(x => x.ResultVariables));

        // Perf: resize the dictionaries to exactly the right bucket sizes.
        _nodeLiveness = new Dictionary<ControlFlowNode<TInstruction>, LivenessData>(_cfg.Nodes.Count);

        int instructionCapacity = 0;
        foreach (var node in _cfg.Nodes)
            instructionCapacity += node.Contents.Instructions.Count;
        _instructionLiveness = new Dictionary<TInstruction, LivenessData>(instructionCapacity);
    }

    public static LivenessAnalysis<TInstruction> Analyze(ControlFlowGraph<TInstruction> cfg, ExitNodeInfo<TInstruction>[] knownExitNodes)
    {
        var analyzer = new LivenessAnalyzer<TInstruction>(cfg, knownExitNodes);
        analyzer.Initialize();
        analyzer.DetermineInOut();
        return analyzer.Construct();
    }

    private LivenessAnalysis<TInstruction> Construct()
    {
        return new LivenessAnalysis<TInstruction>(
            _cfg,
            _variables,
            _instructionLiveness
        );
    }

    private void Initialize()
    {
        // Set all liveness info to empty.
        foreach (var node in _cfg.Nodes)
        {
            _nodeLiveness[node] = LivenessData.Empty;
            for (int i = 0; i < node.Contents.Instructions.Count; i++)
                _instructionLiveness[node.Contents.Instructions[i]] = LivenessData.Empty;
        }

        // Pre-emptively initialize liveness info for all exit nodes.
        foreach (var exitNode in _knownExitNodes)
            _nodeLiveness[exitNode.Node] = _nodeLiveness[exitNode.Node].UnionOut(exitNode.ResultVariables);
    }

    private void DetermineInOut()
    {
        // Maintain reusable GEN and KILL buffers.
        var gen = new List<IVariable>();
        var kill = new List<IVariable>();

        // Reverse postorder for optimal number of iterations.
        var traversal = new DepthFirstTraversal();
        var postOrder = new PostOrderRecorder(traversal);
        traversal.Run(_cfg.EntryPoint!);
        var reversePostOrder = postOrder.GetOrder();

        bool changed = true;
        while (changed)
        {
            changed = false;

            for (int j = 0; j < reversePostOrder.Count; j++)
            {
                var node = (ControlFlowNode<TInstruction>) reversePostOrder[j];

                if (node.Contents.Instructions.Count == 0)
                    ProcessEmptyNode(node, ref changed);
                else
                    ProcessNonEmptyNode(node, gen, kill, ref changed);
            }
        }
    }

    private void ProcessEmptyNode(ControlFlowNode<TInstruction> node, ref bool changed)
    {
        // For empty nodes, just propagate as if a NOP executed.

        var liveness = _nodeLiveness[node];
        var liveIn = liveness.Out;

        var liveOut = liveIn;
        foreach (var successor in node.GetSuccessors())
            liveOut = liveOut.Union(_nodeLiveness[successor].In);

        changed |= Update(_nodeLiveness, node, new LivenessData(liveIn, liveOut));
    }

    private void ProcessNonEmptyNode(ControlFlowNode<TInstruction> node, List<IVariable> gen, List<IVariable> kill, ref bool changed)
    {
        var instructions = node.Contents.Instructions;

        // Propagate exit node liveness into footer liveness.
        var footerLiveness = instructions[instructions.Count - 1];
        changed |= Update(
            _instructionLiveness,
            footerLiveness,
            _instructionLiveness[footerLiveness].UnionOut(_nodeLiveness[node].Out)
        );

        for (int i = instructions.Count - 1; i >= 0; i--)
        {
            var instruction = instructions[i];

            // Obtain GEN and KILL for this instruction.
            gen.Clear();
            _cfg.Architecture.GetReadVariables(instruction, gen);
            kill.Clear();
            _cfg.Architecture.GetWrittenVariables(instruction, kill);

            // Register variables.
            _variables.UnionWith(gen);
            _variables.UnionWith(kill);

            // Get current liveness data for instruction.
            var liveness = _instructionLiveness[instruction];

            // Update IN
            var liveIn = liveness.In;
            liveIn = liveIn.Union(gen);
            foreach (var variable in liveness.Out)
            {
                if (!kill.Contains(variable))
                    liveIn = liveIn.Add(variable);
            }

            // Update OUT
            var liveOut = liveness.Out;
            if (i < node.Contents.Instructions.Count - 1)
            {
                // Only one successor, simply copy IN set of next.
                liveOut = _instructionLiveness[instructions[i + 1]].In;
            }
            else
            {
                // This is the last instruction, union with successors of node.
                foreach (var successor in node.GetSuccessors())
                    liveOut = liveOut.Union(_nodeLiveness[successor].In);
            }

            // If IN or OUT changed, update liveness record.
            changed |= Update(_instructionLiveness, instruction, new LivenessData(liveIn, liveOut));
        }

        // Propagate header liveness into node liveness.
        changed |= Update(_nodeLiveness, node, _nodeLiveness[node].UnionIn(_instructionLiveness[instructions[0]].In));
    }

    private static bool Update<TKey, TValue>(Dictionary<TKey, TValue> collection, TKey node, TValue data)
        where TKey : notnull
        where TValue : notnull
    {
        var original = collection[node];
        if (!original.Equals(data))
        {
            collection[node] = data;
            return true;
        }

        return false;
    }
}