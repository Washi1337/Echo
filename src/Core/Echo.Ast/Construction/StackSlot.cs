using System.Collections.Immutable;
using System.Diagnostics;
using Echo.Code;

namespace Echo.Ast.Construction;

[DebuggerDisplay("{DebuggerDisplay}")]
internal readonly struct StackSlot
{
    public StackSlot(IVariable source)
    {
        Sources = ImmutableHashSet<IVariable>.Empty.Add(source);
    }

    public StackSlot(ImmutableHashSet<IVariable> sources)
    {
        Sources = sources;
    }

    public ImmutableHashSet<IVariable> Sources
    {
        get;
    }

    public StackSlot Union(StackSlot other) => new(Sources.Union(other.Sources));

    public string DebuggerDisplay => $"{{{string.Join(", ", Sources)}}}";
}