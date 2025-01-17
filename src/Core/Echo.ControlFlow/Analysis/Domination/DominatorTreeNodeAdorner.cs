using System.Collections.Generic;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Graphing;
using Echo.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Analysis.Domination;

/// <summary>
/// Implements an adorner that styles a tree node in a dominator tree as a normal control flow node.
/// </summary>
/// <typeparam name="TInstruction">The type of instructions stored in the node.</typeparam>
public class DominatorTreeNodeAdorner<TInstruction> : IDotNodeAdorner 
    where TInstruction : notnull
{
    /// <summary>
    /// Creates an adorner with the default control flow node adorner.
    /// </summary>
    public DominatorTreeNodeAdorner()
        : this(new ControlFlowNodeAdorner<TInstruction>())
    {
    }

    /// <summary>
    /// Creates an adorner with the specified underlying control flow node adorner.
    /// </summary>
    public DominatorTreeNodeAdorner(IDotNodeAdorner nodeAdorner)
    {
        NodeAdorner = nodeAdorner;
    }

    /// <summary>
    /// Gets or sets the underlying control flow node adorner.
    /// </summary>
    public IDotNodeAdorner NodeAdorner { get; set; }

    /// <inheritdoc />
    public IDictionary<string, string>? GetNodeAttributes(INode node, long id)
    {
        return NodeAdorner.GetNodeAttributes(((DominatorTreeNode<TInstruction>) node).OriginalNode, id);
    }
}