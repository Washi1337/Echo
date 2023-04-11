namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Provides a discriminated union describing the result of an exception handler operation, containing either the
    /// next offset to jump to, or a handle to an unhandled exception object.
    /// </summary>
    public readonly struct ExceptionHandlerResult
    {
        private ExceptionHandlerResult(int nextOffset, ObjectHandle exceptionObject)
        {
            NextOffset = nextOffset;
            ExceptionObject = exceptionObject;
        }
        
        /// <summary>
        /// When <see cref="IsSuccess"/> is <c>true</c>, contains the next IL offset to jump to within the current method. 
        /// </summary>
        public int NextOffset
        {
            get;
        }

        /// <summary>
        /// When <see cref="IsSuccess"/> is <c>false</c>, contains the handle to the unhandled exception that was thrown.   
        /// </summary>
        public ObjectHandle ExceptionObject
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether the handler operation was successful.
        /// </summary>
        public bool IsSuccess => ExceptionObject.IsNull;

        /// <summary>
        /// Constructs a new successful exception handler result.
        /// </summary>
        /// <param name="offset">The next IL offset to jump to.</param>
        /// <returns>The result.</returns>
        public static ExceptionHandlerResult Success(int offset) => new(offset, default);

        /// <summary>
        /// Constructs a new unsuccessful exception handler result.
        /// </summary>
        /// <param name="exceptionObject">The handle to the unhandled exception that was thrown.</param>
        /// <returns>The result.</returns>
        public static ExceptionHandlerResult Exception(ObjectHandle exceptionObject) => new(-1, exceptionObject);
    }
}