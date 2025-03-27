using System.Collections.Generic;
using System.IO;
using System.Linq;
using Echo.Code;
using Echo.Graphing;
using Echo.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Analysis.Liveness;

/// <summary>
/// Represents an interference graph, connecting variables that are interfering with each other and thus cannot be
/// allocated the same storage location of a function.
/// </summary>
public class InterferenceGraph : IGraph
{
    /// <summary>
    /// Creates a new empty interference graph.
    /// </summary>
    public InterferenceGraph()
    {
        Nodes = new InterferenceNodeCollection(this);
    }
    
    /// <summary>
    /// Gets a collection of nodes stored in the graph.
    /// </summary>
    public InterferenceNodeCollection Nodes { get; }

    /// <summary>
    /// Constructs the interference graph for the variables used in the provided control flow graph. 
    /// </summary>
    /// <param name="cfg">The control flow graph to analyze.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    /// <returns>The interference graph.</returns>
    public static InterferenceGraph FromFlowGraph<TInstruction>(ControlFlowGraph<TInstruction> cfg) 
        where TInstruction : notnull
    {
        return FromLiveness(LivenessAnalysis.FromFlowGraph(cfg));
    }
    
    /// <summary>
    /// Constructs the interference graph for the variables used in the provided control flow graph. 
    /// </summary>
    /// <param name="cfg">The control flow graph to analyze.</param>
    /// <param name="resultVariables">The set of variables that will be considered alive after returning.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    /// <returns>The interference graph.</returns>
    public static InterferenceGraph FromFlowGraph<TInstruction>(ControlFlowGraph<TInstruction> cfg, IVariable[] resultVariables) 
        where TInstruction : notnull
    {
        return FromLiveness(LivenessAnalysis.FromFlowGraph(cfg, resultVariables));
    }
    
    /// <summary>
    /// Constructs the interference graph for the variables used in the provided control flow graph. 
    /// </summary>
    /// <param name="cfg">The control flow graph to analyze.</param>
    /// <param name="knownExitNodes">A collection of nodes and their result variables that will be considered alive after exiting.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    /// <returns>The interference graph.</returns>
    public static InterferenceGraph FromFlowGraph<TInstruction>(ControlFlowGraph<TInstruction> cfg, ExitNodeInfo<TInstruction>[] knownExitNodes) 
        where TInstruction : notnull
    {
        return FromLiveness(LivenessAnalysis.FromFlowGraph(cfg, knownExitNodes));
    }

    /// <summary>
    /// Constructs the interference graph for the variables used in the provided liveness analysis report. 
    /// </summary>
    /// <param name="analysis">The liveness analysis report to construct the interference graph for.</param>
    /// <typeparam name="TInstruction">The type of instructions stored in the control flow graph.</typeparam>
    /// <returns>The interference graph.</returns>
    public static InterferenceGraph FromLiveness<TInstruction>(LivenessAnalysis<TInstruction> analysis)
        where TInstruction : notnull
    {
        var architecture = analysis.ControlFlowGraph.Architecture;
        
        var result = new InterferenceGraph();

        foreach (var variable in analysis.Variables)
            result.Nodes.GetOrAdd(variable);
        
        var kill = new List<IVariable>();
        
        foreach (var node in analysis.ControlFlowGraph.Nodes)
        {
            for (int i = 0; i < node.Contents.Instructions.Count; i++)
            {
                var instruction = node.Contents.Instructions[i];
                
                // https://cse.sc.edu/~mgv/csce531sp20/notes/mogensen_Ch8_Slides_register-allocation.pdf
                // Variables x interferes with y iff:
                // - x in KILL[i]
                // - y in OUT[i]
                // - x != y
                // - i is not x := y.

                var liveness = analysis[instruction];

                // Get KILL
                kill.Clear();
                architecture.GetWrittenVariables(instruction, kill);

                // See if any x in KILL interfere with any other variables.
                foreach (var x in kill)
                {
                    foreach (var y in liveness.Out)
                    {
                        if (x == y)
                            continue;

                        result.Nodes[x].Interference.Add(result.Nodes[y]);
                    }
                }
            }
        }

        return result;
    }

    IEnumerable<INode> ISubGraph.GetNodes() => Nodes;

    IEnumerable<ISubGraph> ISubGraph.GetSubGraphs() => [];

    IEnumerable<IEdge> IGraph.GetEdges() => Nodes.SelectMany(x => ((INode) x).GetOutgoingEdges());

    /// <summary>
    /// Serializes the interference graph to the provided output stream, in graphviz dot format.
    /// </summary>
    /// <param name="writer">The output stream.</param>
    /// <remarks>To customize the layout of the final graph, use the <see cref="DotWriter"/> class.</remarks>
    public void ToDotGraph(TextWriter writer)
    {
        var dotWriter = new DotWriter(writer)
        {
            NodeAdorner = new VariableNodeAdorner(),
            DirectedGraph = false
        };

        dotWriter.Write(this);
    }

    private sealed class VariableNodeAdorner : IDotNodeAdorner
    {
        public IDictionary<string, string>? GetNodeAttributes(INode node, long id)
        {
            if (node is not InterferenceNode n)
                return null;

            return new Dictionary<string, string>
            {
                ["label"] = n.Variable.Name
            };
        }
    }
}
