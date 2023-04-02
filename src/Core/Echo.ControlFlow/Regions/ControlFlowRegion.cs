using System.Collections.Generic;
using Echo.Graphing;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Provides a base implementation for a region in a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public abstract class ControlFlowRegion<TInstruction> : IControlFlowRegion<TInstruction>
    {
        /// <inheritdoc />
        public ControlFlowGraph<TInstruction> ParentGraph
        {
            get
            {
                var region = ParentRegion;
                while (true)
                {
                    switch (region)
                    {
                        case null:
                            return null;
                        case ControlFlowGraph<TInstruction> graph:
                            return graph;
                        default:
                            region = region.ParentRegion;
                            break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public IControlFlowRegion<TInstruction> ParentRegion
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets a user-defined tag that is assigned to this region. 
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        /// <inheritdoc />
        public abstract ControlFlowNode<TInstruction> GetEntrypoint();

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
        public abstract IEnumerable<ControlFlowNode<TInstruction>> GetNodes();

        /// <inheritdoc />
        IEnumerable<INode> ISubGraph.GetNodes() => GetNodes();

        /// <inheritdoc />
        public abstract IEnumerable<ControlFlowRegion<TInstruction>> GetSubRegions();

        /// <inheritdoc />
        IEnumerable<ISubGraph> ISubGraph.GetSubGraphs() => GetSubRegions();

        /// <inheritdoc />
        public abstract bool RemoveNode(ControlFlowNode<TInstruction> node);

        /// <inheritdoc />
        public virtual IEnumerable<ControlFlowNode<TInstruction>> GetSuccessors()
        {
            foreach (var node in GetNodes())
            {
                foreach (var candidate in node.GetSuccessors())
                {
                    if (!candidate.IsInRegion(this))
                        yield return candidate;
                }
            }
        }
    }
}