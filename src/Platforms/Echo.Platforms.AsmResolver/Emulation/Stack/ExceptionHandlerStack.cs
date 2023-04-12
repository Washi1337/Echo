namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Implements the exception handling mechanism within a single managed method.
    /// </summary>
    public class ExceptionHandlerStack : IndexableStack<ExceptionHandlerFrame>
    {
        /// <summary>
        /// Registers an exception, unwinds the exception handler stack and determines where to jump to within the
        /// current method.
        /// </summary>
        /// <param name="exceptionObject">The handle to the exception that was thrown.</param>
        /// <returns>
        /// The offset to jump to next within the method, or the exception object if the exception was not handled
        /// by the method.
        /// </returns>
        public ExceptionHandlerResult RegisterException(ObjectHandle exceptionObject)
        {
            // When an exception occurs, we need to find the handler that should be jumped to next. 
            
            while (Count > 0)
            {
                var currentFrame = Peek();

                // Try let the current frame determine where to jump to.
                int? offset = currentFrame.RegisterException(exceptionObject);
                if (offset.HasValue)
                    return ExceptionHandlerResult.Success(offset.Value);

                // If the current frame could not handle the exception, see if we can find another frame that can.
                Pop();
            }

            // No frame was able to handle the exception, report as unhandled.
            return ExceptionHandlerResult.Exception(exceptionObject);
        }
        
        /// <summary>
        /// Safely leaves one or more exception handlers that are currently on the stack.
        /// </summary>
        /// <param name="targetOffset">The offset as indicated by the leave instruction.</param>
        /// <returns>The offset to jump to next.</returns>
        public int Leave(int targetOffset)
        {
            // Leaving an EH implies that we are leaving the EH safely without any unhandled exceptions.
            // We thus just need to find the next address to jump to.
            
            // Try let the current frame determine where to jump to.
            var currentFrame = Peek();
            int offset = currentFrame.Leave(targetOffset);
            
            // If the target offset falls within the current frame, we are jumping to one of our handlers.
            if (currentFrame.ContainsOffset(offset))
                return offset;

            // If the target offset falls outside the frame, we are actually leaving this frame.
            Pop();
            return targetOffset;
        }

        /// <summary>
        /// Exits a filter clause with the provided result, and unwinds the exception handler stack if necessary.
        /// </summary>
        /// <param name="result">The result of the filter expression.</param>
        /// <returns>
        /// The offset to jump to next within the method, or the exception object if the exception was not handled
        /// by the method.
        /// </returns>
        public ExceptionHandlerResult EndFilter(bool result)
        {
            // An endfilter only happens within the top-most frame when an exception occurred.
            var currentFrame = Peek();
            
            // Try let the current frame determine where to jump to.
            int? offset = currentFrame.EndFilter(result);
            if (offset.HasValue)
                return ExceptionHandlerResult.Success(offset.Value);
            
            // If the current frame could not handle the exception, bubble it up the stack.
            return RegisterException(Pop().ExceptionObject);
        }
        
        /// <summary>
        /// Exits a finally clause, and unwinds the exception handler stack if necessary.
        /// </summary>
        /// <returns>
        /// The offset to jump to next within the method, or the exception object if the exception was not handled
        /// by the method.
        /// </returns>
        public ExceptionHandlerResult EndFinally()
        {
            // An endfinally occurs when we leave a handler either with or without an unhandled exception.
            
            // Endfinally always leaves the current EH frame.
            var currentFrame = Pop();

            // Let the current frame decide where to jump to next.  
            int? offset = currentFrame.EndFinally();
            if (offset.HasValue)
            {
                // We are safely leaving the frame.
                return ExceptionHandlerResult.Success(offset.Value);
            }

            // If the frame could not decide where to jump to, it means we left the protected region unexpectedly with
            // an exception. Bubble it up.
            return RegisterException(currentFrame.ExceptionObject);
        }
    }
}