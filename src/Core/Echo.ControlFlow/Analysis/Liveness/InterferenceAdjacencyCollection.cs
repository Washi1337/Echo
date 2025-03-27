using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Echo.Code;

namespace Echo.ControlFlow.Analysis.Liveness;

/// <summary>
/// Represents an adjacency list of a node in an interference graph.
/// </summary>
public class InterferenceAdjacencyCollection : ICollection<InterferenceNode>
{
    private readonly InterferenceNode _owner;
    private readonly HashSet<InterferenceNode> _nodes = [];

    internal InterferenceAdjacencyCollection(InterferenceNode owner)
    {
        _owner = owner;
    }

    /// <inheritdoc />
    public int Count => _nodes.Count;

    bool ICollection<InterferenceNode>.IsReadOnly => false;

    /// <summary>
    /// Adds a node to the neighbors list.
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns><c>true</c> if the node was added, <c>false</c> if the node was already part of the neighborhood.</returns>
    public bool Add(InterferenceNode node)
    {
        if (node.ParentGraph is null)
            throw new InvalidOperationException("Cannot add interference to a node that is not part of a graph.");
        if (node.ParentGraph != _owner.ParentGraph)
            throw new ArgumentException("Node is not part of the same graph.");
        if (node == _owner)
            return false;

        if (_nodes.Add(node))
        {
            node.Interference.Add(_owner);
            return true;
        }

        return false;
    }
    
    void ICollection<InterferenceNode>.Add(InterferenceNode item) => _nodes.Add(item);

    /// <summary>
    /// Removes a node from the neighbors list.
    /// </summary>
    /// <param name="item">The node to remove.</param>
    /// <returns><c>true</c> if the node was present in the neighbors, <c>false</c> otherwise.</returns>
    public bool Remove(InterferenceNode item)
    {
        if (_nodes.Remove(item))
        {
            item.Interference.Remove(_owner);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var node in _nodes.ToArray())
            Remove(node);
    }

    /// <inheritdoc />
    public bool Contains(InterferenceNode item) => _nodes.Contains(item);

    /// <inheritdoc />
    public void CopyTo(InterferenceNode[] array, int arrayIndex) => _nodes.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<InterferenceNode> GetEnumerator() => _nodes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _nodes).GetEnumerator();

}