using System;
using System.Collections.Generic;
using Echo.ControlFlow.Collections;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Represents a simple unordered region defining an inner scope in the control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public class ScopeRegion<TInstruction> : ControlFlowRegion<TInstruction>
    {
        private ControlFlowNode<TInstruction> _entrypoint;

        /// <summary>
        /// Creates a new instance of the <see cref="ScopeRegion{TInstruction}"/> class.
        /// </summary>
        public ScopeRegion()
        {
            Regions = new RegionCollection<TInstruction, ControlFlowRegion<TInstruction>>(this);
            Nodes = new RegionNodeCollection<TInstruction>(this);
        }

        /// <summary>
        /// Gets or sets the first node that is executed in the region.
        /// </summary>
        public ControlFlowNode<TInstruction> Entrypoint
        {
            get => _entrypoint;
            set => _entrypoint = value;
        }

        /// <summary>
        /// Gets a collection of top-level nodes that this region consists of.
        /// </summary>
        /// <remarks>
        /// This collection does not include any nodes in the nested sub regions.
        /// </remarks>
        public RegionNodeCollection<TInstruction> Nodes
        {
            get;
        }

        /// <summary>
        /// Gets a collection of nested sub regions that this region defines.
        /// </summary>
        public RegionCollection<TInstruction, ControlFlowRegion<TInstruction>> Regions
        {
            get;
        }

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
        public override ControlFlowNode<TInstruction> GetEntrypoint() => Entrypoint;

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