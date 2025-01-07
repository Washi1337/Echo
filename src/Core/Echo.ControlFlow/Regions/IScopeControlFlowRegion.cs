using System.Collections.Generic;
using Echo.ControlFlow.Collections;

namespace Echo.ControlFlow.Regions;

/// <summary>
/// Represents a scope of nodes and regions.
/// </summary>
/// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
public interface IScopeControlFlowRegion<TInstruction> : IControlFlowRegion<TInstruction>
    where TInstruction : notnull
{
    /// <summary>
    /// Gets a collection of top-level nodes stored in this region.
    /// </summary>
    public ICollection<ControlFlowNode<TInstruction>> Nodes { get; }

    /// <summary>
    /// Gets a collection of nested subregions that this region defines.
    /// </summary>
    public RegionCollection<TInstruction, ControlFlowRegion<TInstruction>> Regions { get; }
}