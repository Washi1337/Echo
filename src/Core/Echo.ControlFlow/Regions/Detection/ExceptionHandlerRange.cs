using System;
using Echo.Code;

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
        public ExceptionHandlerRange(AddressRange protectedRange, AddressRange handlerRange)
            : this(protectedRange, handlerRange, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRange"/> structure.
        /// </summary>
        /// <param name="protectedRange">The range indicating the code that is protected by the handler.</param>
        /// <param name="prologueRange">The range indicating the prologue range that precedes the handler.</param>
        /// <param name="handlerRange">The range indicating the handler code.</param>
        public ExceptionHandlerRange(AddressRange protectedRange, AddressRange prologueRange, AddressRange handlerRange)
            : this(protectedRange, prologueRange, handlerRange, null)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRange"/> structure.
        /// </summary>
        /// <param name="protectedRange">The range indicating the code that is protected by the handler.</param>
        /// <param name="prologueRange">The range indicating the prologue range that precedes the handler.</param>
        /// <param name="handlerRange">The range indicating the handler code.</param>
        /// <param name="userData">A user defined tag that is added to the exception handler.</param>
        public ExceptionHandlerRange(
            AddressRange protectedRange,
            AddressRange prologueRange,
            AddressRange handlerRange,
            object userData) 
            : this(protectedRange, prologueRange, handlerRange, AddressRange.NilRange, userData)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRange"/> structure.
        /// </summary>
        /// <param name="protectedRange">The range indicating the code that is protected by the handler.</param>
        /// <param name="prologueRange">The range indicating the prologue range that precedes the handler.</param>
        /// <param name="handlerRange">The range indicating the handler code.</param>
        /// <param name="epilogueRange">The range indicating the epilogue range that proceeds the handler.</param>
        public ExceptionHandlerRange(
            AddressRange protectedRange,
            AddressRange prologueRange,
            AddressRange handlerRange,
            AddressRange epilogueRange)
            : this(protectedRange, prologueRange, handlerRange, epilogueRange, null)
        {
        }
        
        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRange"/> structure.
        /// </summary>
        /// <param name="protectedRange">The range indicating the code that is protected by the handler.</param>
        /// <param name="prologueRange">The range indicating the prologue range that precedes the handler.</param>
        /// <param name="handlerRange">The range indicating the handler code.</param>
        /// <param name="epilogueRange">The range indicating the epilogue range that proceeds the handler.</param>
        /// <param name="userData">A user defined tag that is added to the exception handler.</param>
        public ExceptionHandlerRange(
            AddressRange protectedRange,
            AddressRange prologueRange,
            AddressRange handlerRange,
            AddressRange epilogueRange,
            object userData)
            : this(protectedRange, handlerRange, userData)
        {
            PrologueRange = prologueRange;
            EpilogueRange = epilogueRange;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ExceptionHandlerRange"/> structure.
        /// </summary>
        /// <param name="protectedRange">The range indicating the code that is protected by the handler.</param>
        /// <param name="handlerRange">The range indicating the handler code.</param>
        /// <param name="userData">A user defined tag that is added to the exception handler.</param>
        public ExceptionHandlerRange(AddressRange protectedRange, AddressRange handlerRange, object userData)
        {
            ProtectedRange = protectedRange;
            PrologueRange = AddressRange.NilRange;
            HandlerRange = handlerRange;
            EpilogueRange = AddressRange.NilRange;
            UserData = userData;
        }
        
        /// <summary>
        /// Gets the address range indicating the start and end of the code that is protected by a handler.
        /// </summary>
        public AddressRange ProtectedRange
        {
            get;
        }

        /// <summary>
        /// Gets the address range indicating the start and end of the code that is executed before transferring
        /// control to the <see cref="HandlerRange"/>.
        /// </summary>
        /// <remarks>A good example would be exception filters in CIL.</remarks>
        public AddressRange PrologueRange
        {
            get;
        }

        /// <summary>
        /// Gets the address range indicating the start and end of the handler code.
        /// </summary>
        public AddressRange HandlerRange
        {
            get;
        }

        /// <summary>
        /// Gets the address range indicating the start and end of the code that is
        /// executed after the <see cref="HandlerRange"/>.
        /// </summary>
        public AddressRange EpilogueRange
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
        public bool Equals(in ExceptionHandlerRange other) => 
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