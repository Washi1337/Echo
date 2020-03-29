using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Provides a base implementation for a region in a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public abstract class ControlFlowRegion<TInstruction> : IControlFlowRegion<TInstruction>
    {
        /// <inheritdoc />
        public IControlFlowRegion<TInstruction> ParentRegion
        {
            get;
            internal set;
        }

        /// <inheritdoc />
        public virtual ControlFlowNode<TInstruction> GetNodeByOffset(long offset)
        {
            foreach (var region in GetSubRegions())
            {
                var node = region.GetNodeByOffset(offset);
                if (node != null)
                    return node;
            }

            return null;
        }

        /// <inheritdoc />
        INode ISubGraph.GetNodeById(long id) => GetNodeByOffset(id);

        /// <inheritdoc />
        public abstract IEnumerable<ControlFlowNode<TInstruction>> GetNodes();

        /// <inheritdoc />
        IEnumerable<INode> ISubGraph.GetNodes() => GetNodes();

        /// <inheritdoc />
        public abstract IEnumerable<ControlFlowRegion<TInstruction>> GetSubRegions();
    }
}