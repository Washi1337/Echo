using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Echo.Code;

namespace Echo.ControlFlow.Analysis.Liveness;

/// <summary>
/// Represents a collection of variables (nodes) in an interference graph.
/// </summary>
public class InterferenceNodeCollection : ICollection<InterferenceNode>
{
    private readonly InterferenceGraph _owner;
    private readonly Dictionary<IVariable, InterferenceNode> _nodes = new();

    internal InterferenceNodeCollection(InterferenceGraph owner)
    {
        _owner = owner;
    }

    /// <inheritdoc />
    public int Count => _nodes.Count;

    bool ICollection<InterferenceNode>.IsReadOnly => false;

    /// <summary>
    /// Gets the node represented by the provided variable.
    /// </summary>
    /// <param name="variable">The variable.</param>
    public InterferenceNode this[IVariable variable] => _nodes[variable];

    /// <summary>
    /// Gets or adds the node for the provided variable.
    /// </summary>
    /// <param name="variable">The variable/</param>
    /// <returns>The node.</returns>
    public InterferenceNode GetOrAdd(IVariable variable)
    {
        if (!_nodes.TryGetValue(variable, out var node))
        {
            node = new InterferenceNode(variable);
            Add(node);
        }
        
        return node;
    }

    /// <summary>
    /// Gets the node for the provided variable.
    /// </summary>
    /// <param name="variable">The variable</param>
    /// <param name="node">The node, or <c>null</c> if none was found.</param>
    /// <returns><c>true</c> if the node was found, <c>false</c> otherwise.</returns>
    public bool TryGet(IVariable variable, [NotNullWhen(true)] out InterferenceNode? node)
    {
        return _nodes.TryGetValue(variable, out node);
    }

    /// <inheritdoc />
    public void Add(InterferenceNode item)
    {
        if (item.ParentGraph == _owner)
            return;
        if (item.ParentGraph is not null)
            throw new ArgumentException("Node is already added to another graph.");
        if (_nodes.ContainsKey(item.Variable))
            throw new ArgumentException("Variable is already added to the graph.");

        _nodes.Add(item.Variable, item);
        item.ParentGraph = _owner;
    }

    /// <inheritdoc />
    public void Clear()
    {
        foreach (var item in _nodes.Values)
            item.Disconnect();
        _nodes.Clear();
    }

    /// <summary>
    /// Determines whether the variable is part of the interference graph.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns><c>true</c> if there is a node representing the variable, <c>false</c> otherwise.</returns>
    public bool Contains(IVariable variable) => _nodes.ContainsKey(variable);

    /// <inheritdoc />
    public bool Contains(InterferenceNode item) => _nodes.TryGetValue(item.Variable, out var node) && node == item;

    /// <inheritdoc />
    public void CopyTo(InterferenceNode[] array, int arrayIndex) => _nodes.Values.CopyTo(array, arrayIndex);

    /// <summary>
    /// Removes a variable from the interference graph.
    /// </summary>
    /// <param name="variable">The variable.</param>
    /// <returns><c>true</c> if the variable was removed, <c>false</c> if the variable was not part of the graph.</returns>
    public bool Remove(IVariable variable)
    {
        if (_nodes.TryGetValue(variable, out var node))
        {
            RemoveInternal(node);
            return true;
        }

        return false;
    }
    
    /// <inheritdoc />
    public bool Remove(InterferenceNode item)
    {
        if (_nodes.TryGetValue(item.Variable, out var node) && node == item)
        {
            RemoveInternal(item);
            return true;
        }

        return false;
    }

    private void RemoveInternal(InterferenceNode item)
    {
        item.Disconnect();
        item.ParentGraph = null;
        _nodes.Remove(item.Variable);
    }
        
    /// <inheritdoc />
    public IEnumerator<InterferenceNode> GetEnumerator() => _nodes.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}