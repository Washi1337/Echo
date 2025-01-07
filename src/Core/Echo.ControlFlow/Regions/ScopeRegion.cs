using System.Collections.Generic;
using Echo.ControlFlow.Collections;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Represents a simple unordered region defining an inner scope in the control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public class ScopeRegion<TInstruction> : ControlFlowRegion<TInstruction>, IScopeControlFlowRegion<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new empty scope region with no extra semantics attached.
        /// </summary>
        public ScopeRegion()
            : this(ScopeRegionType.None)
        {
        }

        /// <summary>
        /// Creates a new empty scope region of the provided scope type.
        /// </summary>
        /// <param name="type">The type.</param>
        public ScopeRegion(ScopeRegionType type)
        {
            Regions = new RegionCollection<TInstruction, ControlFlowRegion<TInstruction>>(this);
            Nodes = new RegionNodeCollection<TInstruction>(this);
            ScopeType = type;
        }

        /// <summary>
        /// Gets or sets the first node that is executed in the region.
        /// </summary>
        public ControlFlowNode<TInstruction>? EntryPoint { get; set; }

        /// <summary>
        /// Gets a collection of top-level nodes that this region consists of.
        /// </summary>
        /// <remarks>
        /// This collection does not include any nodes in the nested sub regions.
        /// </remarks>
        public RegionNodeCollection<TInstruction> Nodes { get; }

        ICollection<ControlFlowNode<TInstruction>> IScopeControlFlowRegion<TInstruction>.Nodes => Nodes;

        /// <summary>
        /// Gets a collection of nested subregions that this region defines.
        /// </summary>
        public RegionCollection<TInstruction, ControlFlowRegion<TInstruction>> Regions { get; }

        /// <summary>
        /// Gets or sets the type of scope.
        /// </summary>
        public ScopeRegionType ScopeType { get; set; }

        /// <inheritdoc />
        public override IEnumerable<ControlFlowNode<TInstruction>> GetNodes()
        {
            foreach (var node in Nodes)
                yield return node;

            foreach (var region in Regions)
            {
                foreach (var node in region.GetNodes())
                    yield return node;
            }
        }

        /// <inheritdoc />
        public override ControlFlowNode<TInstruction>? GetEntryPoint() => EntryPoint;

        /// <inheritdoc />
        public override IEnumerable<ControlFlowRegion<TInstruction>> GetSubRegions() => Regions;

        /// <inheritdoc />
        public override bool RemoveNode(ControlFlowNode<TInstruction> node)
        {
            if (Nodes.Remove(node))
                return true;

            foreach (var region in Regions)
            {
                if (region.RemoveNode(node))
                    return true;
            }

            return false;
        }
    }
}