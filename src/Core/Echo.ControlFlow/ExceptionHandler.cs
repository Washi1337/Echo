using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;

namespace Echo.ControlFlow
{

    /// <summary>
    /// Provides a base implementation of an exception handler.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to use.</typeparam>
    public class ExceptionHandler<TInstruction> : IExceptionHandler
        where TInstruction : IInstruction
    {
        /// <summary>
        /// Gets the graph segment that is protected by the exception handler.
        /// </summary>
        public GraphSegment<TInstruction> Try
        {
            get;
        } = new GraphSegment<TInstruction>();

        /// <summary>
        /// Gets the graph segment that handles any exceptions that might occur in the protected region.
        /// </summary>
        public GraphSegment<TInstruction> Handler
        {
            get;
        } = new GraphSegment<TInstruction>();

        /// <summary>
        /// Gets a collection of all nodes present in the exception handler.
        /// </summary>
        /// <returns>The nodes.</returns>
        /// <remarks>
        /// This collection first returns all nodes within the protected region, and then the nodes of the handler segment.
        /// </remarks>
        public IEnumerable<Node<TInstruction>> GetNodes() => Try.Nodes.Union(Handler.Nodes);
        
        INode IGraphSegment.Entrypoint => Try.Entrypoint;
        
        IGraphSegment IExceptionHandler.Try => Try;
        
        IGraphSegment IExceptionHandler.Handler => Handler;

        IEnumerable<INode> IGraphSegment.GetNodes() => GetNodes();
    }
}