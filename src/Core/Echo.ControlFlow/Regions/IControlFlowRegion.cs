using System.Collections.Generic;
using Echo.ControlFlow.Collections;
using Echo.Graphing;

namespace Echo.ControlFlow.Regions
{
    /// <summary>
    /// Provides members for describing a region in a control flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public interface IControlFlowRegion<TInstruction> : ISubGraph
        where TInstruction : notnull
    {
        /// <summary>
        /// Gets the parent graph this region is part of.
        /// </summary>
        ControlFlowGraph<TInstruction>? ParentGraph
        {
            get;
        }

        /// <summary>
        /// Gets the parent region that this region is part of. 
        /// </summary>
        /// <remarks>
        /// When this property is set to <c>null</c> this region is the root.
        /// </remarks>
        IControlFlowRegion<TInstruction>? ParentRegion
        {
            get;
        }
        
        /// <summary>
        /// Obtains the first node that is executed in the region (if available).
        /// </summary>
        /// <returns>The node, or <c>null</c> if no entrypoint was specified.</returns>
        ControlFlowNode<TInstruction>? GetEntryPoint();
        
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
        /// <returns>The node, or <c>null</c> if no node was found with the provided offset.</returns>
        ControlFlowNode<TInstruction>? GetNodeByOffset(long offset);

        /// <summary>
        /// Removes the node from the region.
        /// </summary>
        /// <param name="node">The node to remove.</param>
        /// <returns><c>true</c> if the node was found and removed, <c>false</c> otherwise.</returns>
        bool RemoveNode(ControlFlowNode<TInstruction> node);

        /// <summary>
        /// Gets the nodes that are immediate successors of any node in this region.
        /// </summary>
        /// <returns>The nodes.</returns>
        IEnumerable<ControlFlowNode<TInstruction>> GetSuccessors();
    }

    /// <summary>
    /// Represents a scope of regions. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public interface IScopeControlFlowRegion<TInstruction> : IControlFlowRegion<TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Gets a collection of nested sub regions that this region defines.
        /// </summary>
        public RegionCollection<TInstruction, ControlFlowRegion<TInstruction>> Regions
        {
            get;
        }
    }
    
    /// <summary>
    /// Provides extensions to the <see cref="IControlFlowRegion{TInstruction}"/> interface.
    /// </summary>
    public static class ControlFlowRegionExtensions
    {
        /// <summary>
        /// Obtains the parent exception handler region that this region resides in (if any).
        /// </summary>
        /// <returns>
        /// The parent exception handler region, or <c>null</c> if the region is not part of any exception handler.
        /// </returns>
        public static ExceptionHandlerRegion<TInstruction>? GetParentExceptionHandler<TInstruction>(this IControlFlowRegion<TInstruction> self)
            where TInstruction : notnull
        {
            return GetParentRegion<TInstruction, ExceptionHandlerRegion<TInstruction>>(self);
        }

        /// <summary>
        /// Obtains the parent handler region that this region resides in (if any).
        /// </summary>
        /// <returns>
        /// The parent exception handler region, or <c>null</c> if the region is not part of any exception handler.
        /// </returns>
        public static HandlerRegion<TInstruction>? GetParentHandler<TInstruction>(this IControlFlowRegion<TInstruction> self)
            where TInstruction : notnull
        {
            return GetParentRegion<TInstruction, HandlerRegion<TInstruction>>(self);
        }

        /// <summary>
        /// Obtains the parent region of a specific type that this region resides in (if any).
        /// </summary>
        /// <returns>
        /// The parent region, or <c>null</c> if the region is not part of any region of type <typeparamref name="TRegion"/>.
        /// </returns>
        private static TRegion? GetParentRegion<TInstruction, TRegion>(IControlFlowRegion<TInstruction> self)
            where TInstruction : notnull
            where TRegion : class, IControlFlowRegion<TInstruction>
        {
            var region = self.ParentRegion;
            
            while (region is not null)
            {
                if (region is TRegion ehRegion)
                    return ehRegion;

                region = region.ParentRegion;
            }

            return null;
        }
    }
}