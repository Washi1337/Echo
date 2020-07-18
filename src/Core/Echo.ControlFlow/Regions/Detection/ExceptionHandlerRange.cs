using System;
using Echo.Core.Code;

namespace Echo.ControlFlow.Regions.Detection
{
    /// <summary>
    /// Represents an address range of code that is protected from exceptions by a handler block.  
    /// </summary>
    public readonly struct ExceptionHandlerRange : IComparable<ExceptionHandlerRange>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRange"/> structure.
        /// </summary>
        /// <param name="protectedRange">The range indicating the code that is protected by the handler.</param>
        /// <param name="handlerRange">The range indicating the handler code.</param>
        public ExceptionHandlerRange(IAddressRange protectedRange, IAddressRange handlerRange)
        {
            ProtectedRange = protectedRange;
            HandlerRange = handlerRange;
            UserData = null;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRange"/> structure.
        /// </summary>
        /// <param name="protectedRange">The range indicating the code that is protected by the handler.</param>
        /// <param name="handlerRange">The range indicating the handler code.</param>
        /// <param name="userData">A user defined tag that is added to the exception handler.</param>
        public ExceptionHandlerRange(IAddressRange protectedRange, IAddressRange handlerRange, object userData)
        {
            ProtectedRange = protectedRange;
            HandlerRange = handlerRange;
            UserData = userData;
        }
        
        /// <summary>
        /// Gets the address range indicating the start and end of the code that is protected by a handler.
        /// </summary>
        public IAddressRange ProtectedRange
        {
            get;
        }

        /// <summary>
        /// Gets the address range indicating the start and end of the handler code.
        /// </summary>
        public IAddressRange HandlerRange
        {
            get;
        }

        /// <summary>
        /// Gets a user defined tag that is added to the exception handler.
        /// </summary>
        public object UserData
        {
            get;
        }

        /// <summary>
        /// Determines whether two exception handlers are considered equal.
        /// </summary>
        /// <param name="other">The other exception handler.</param>
        /// <returns><c>true</c> if the handler is equal, <c>false</c> otherwise.</returns>
        public bool Equals(ExceptionHandlerRange other) => 
            ProtectedRange.Equals(other.ProtectedRange) && HandlerRange.Equals(other.HandlerRange);

        /// <inheritdoc />
        public override bool Equals(object obj) => 
            obj is ExceptionHandlerRange other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (ProtectedRange.GetHashCode() * 397) ^ HandlerRange.GetHashCode();
            }
        }

        /// <inheritdoc />
        public int CompareTo(ExceptionHandlerRange other)
        {
            // Most common shortcut: If the protected range is equal, we just sort by handler start.
            if (ProtectedRange == other.ProtectedRange)
                return HandlerRange.Start.CompareTo(other.HandlerRange.Start);

            // Check if current EH encloses the other, and if so, order the current EH before the other.
            if (ProtectedRange.Contains(other.ProtectedRange))
                return -1;
            if (other.ProtectedRange.Contains(ProtectedRange))
                return 1;
                
            if (HandlerRange.Contains(other.HandlerRange))
                return -1;
            if (other.HandlerRange.Contains(HandlerRange))
                return 1;

            // EH is not overlapping in any way, just order by start offset.
            return ProtectedRange.Start.CompareTo(other.ProtectedRange.Start);
        }

        /// <inheritdoc />
        public override string ToString() => 
            $"{nameof(ProtectedRange)}: {ProtectedRange}, {nameof(HandlerRange)}: {HandlerRange}";
    }
}