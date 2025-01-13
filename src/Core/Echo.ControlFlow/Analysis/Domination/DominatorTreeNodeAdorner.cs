using System.Collections.Generic;
using Echo.ControlFlow.Serialization.Dot;
using Echo.Graphing;
using Echo.Graphing.Serialization.Dot;

namespace Echo.ControlFlow.Analysis.Domination;

public class DominatorTreeNodeAdorner<TInstruction> : IDotNodeAdorner 
    where TInstruction : notnull
{
    public DominatorTreeNodeAdorner()
        : this(new ControlFlowNodeAdorner<TInstruction>())
    {
    }

    public DominatorTreeNodeAdorner(IDotNodeAdorner nodeAdorner)
    {
        NodeAdorner = nodeAdorner;
    }

    public IDotNodeAdorner NodeAdorner { get; set; }

    /// <inheritdoc />
    public IDictionary<string, string>? GetNodeAttributes(INode node, long id)
    {
        return NodeAdorner.GetNodeAttributes(((DominatorTreeNode<TInstruction>) node).OriginalNode, id);
    }
}