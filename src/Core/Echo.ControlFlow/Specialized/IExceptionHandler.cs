namespace Echo.ControlFlow.Specialized
{
    /// <summary>
    /// Represents a single exception handler region in a control flow graph, consisting of a segment that is protected
    /// by the handler, and the segment containing the code that handles any exceptions that might occur in the protected
    /// region. 
    /// </summary>
    public interface IExceptionHandler : IGraphSegment
    {
        /// <summary>
        /// Gets the graph segment that is protected by the exception handler.
        /// </summary>
        IGraphSegment Try
        {
            get;
        }

        /// <summary>
        /// Gets the graph segment that handles any exceptions that might occur in the protected region.
        /// </summary>
        IGraphSegment Handler
        {
            get;
        }
    }
}