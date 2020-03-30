using System.Collections.Generic;
using Echo.Core.Graphing;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Provides members for describing a region in a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public interface IControlFlowRegion<TInstruction> : ISubGraph
    {
        ControlFlowGraph<TInstruction> ParentGraph
        {
            get;
        }

        /// <summary>
        /// Gets the parent region that this region is part of. 
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>null</c> this region is the root.
        /// </remarks>
        IControlFlowRegion<TInstruction> ParentRegion
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of all nested regions defined in this region.
        /// </summary>
        /// <returns>The sub regions.</returns>
        IEnumerable<ControlFlowRegion<TInstruction>> GetSubRegions();
        
        /// <summary>
        /// Gets a collection of all nodes in the control flow graph region. This includes all nodes in the nested
        /// regions. 
        /// </summary>
        /// <returns>The nodes.</returns>
        new IEnumerable<ControlFlowNode<TInstruction>> GetNodes();

        /// <summary>
        /// Searches for a node in the control flow graph with the provided offset or identifier.
        /// </summary>
        /// <param name="offset">The offset of the node to find.</param>
        /// <returns>The node.</returns>
        ControlFlowNode<TInstruction> GetNodeByOffset(long offset);

    }
}