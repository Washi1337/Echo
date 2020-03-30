using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.Core.Graphing;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Represents a simple unordered region in a control flow graph. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public class BasicControlFlowRegion<TInstruction> : ControlFlowRegion<TInstruction>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="BasicControlFlowRegion{TInstruction}"/> class.
        /// </summary>
        public BasicControlFlowRegion()
        {
            Regions = new RegionCollection<TInstruction>(this);
            Nodes = new RegionNodeCollection<TInstruction>(this);
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
        public RegionCollection<TInstruction> Regions
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