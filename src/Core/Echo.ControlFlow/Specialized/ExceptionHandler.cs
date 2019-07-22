using System.Collections.Generic;
using System.Linq;

namespace Echo.ControlFlow.Specialized
{

    /// <summary>
    /// Provides a base implementation of an exception handler.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to use.</typeparam>
    public class ExceptionHandler<TInstruction> : IExceptionHandler
    {
        /// <summary>
        /// Gets the graph segment that is protected by the exception handler.
        /// </summary>
        public ControlFlowGraphRegion<TInstruction> Try
        {
            get;
        } = new ControlFlowGraphRegion<TInstruction>();

        /// <summary>
        /// Gets the graph segment that handles any exceptions that might occur in the protected region.
        /// </summary>
        public ControlFlowGraphRegion<TInstruction> Handler
        {
            get;
        } = new ControlFlowGraphRegion<TInstruction>();

        /// <summary>
        /// Gets a collection of all nodes present in the exception handler.
        /// </summary>
        /// <returns>The nodes.</returns>
        /// <remarks>
        /// This collection first returns all nodes within the protected region, and then the nodes of the handler segment.
        /// </remarks>
        public IEnumerable<Node<BasicBlock<TInstruction>>> GetNodes() => Try.Nodes.Union(Handler.Nodes);
        
        INode IGraphSegment.Entrypoint => Try.Entrypoint;
        
        IGraphSegment IExceptionHandler.Try => Try;
        
        IGraphSegment IExceptionHandler.Handler => Handler;

        IEnumerable<INode> IGraphSegment.GetNodes() => GetNodes();
    }
}