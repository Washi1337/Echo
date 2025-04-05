using System.Collections.Generic;
using System.Linq;
using Echo.Code;
using Echo.Graphing;

namespace Echo.ControlFlow.Analysis.Liveness;

/// <summary>
/// Represents a single node in an interference graph.
/// </summary>
public class InterferenceNode : INode
{
    /// <summary>
    /// Creates a new interference node.
    /// </summary>
    /// <param name="variable">The variable to represent.</param>
    public InterferenceNode(IVariable variable)
    {
        Variable = variable;
        Interference = new InterferenceAdjacencyCollection(this);
    }
    
    /// <summary>
    /// Gets the parent graph the node is added to.
    /// </summary>
    public InterferenceGraph? ParentGraph
    {
        get;
        internal set;
    }
    
    /// <summary>
    /// Gets the variable that is represented by this node.
    /// </summary>
    public IVariable Variable { get; }
    
    /// <summary>
    /// Gets a collection of variable (nodes) the variable interferes with.
    /// </summary>
    public InterferenceAdjacencyCollection Interference { get; }

    /// <summary>
    /// Gets the number of variables the variable interferes with.
    /// </summary>
    public int Degree => Interference.Count;
    
    int INode.InDegree => Degree;

    int INode.OutDegree => Degree;

    /// <summary>
    /// Disconnects the node from all variables in the graph.
    /// </summary>
    public void Disconnect() => Interference.Clear();

    IEnumerable<IEdge> INode.GetIncomingEdges() => Interference.Select(x => new Edge(this, x));

    IEnumerable<IEdge> INode.GetOutgoingEdges() => Interference.Select(x => new Edge(this, x));

    IEnumerable<INode> INode.GetPredecessors() => Interference;

    IEnumerable<INode> INode.GetSuccessors() => Interference;

    bool INode.HasPredecessor(INode node) => node is InterferenceNode n && Interference.Contains(n);

    bool INode.HasSuccessor(INode node) => node is InterferenceNode n && Interference.Contains(n);

    /// <inheritdoc />
    public override string ToString() => $"{Variable}";
}