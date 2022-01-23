using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Represents a single slot in the evaluation stack.
    /// </summary>
    public readonly struct StackSlot
    {
        /// <summary>
        /// Creates a new stack slot.
        /// </summary>
        /// <param name="contents">The value stored in the slot.</param>
        /// <param name="typeHint">A type hint indicating how this value was pushed.</param>
        public StackSlot(BitVector contents, StackSlotTypeHint typeHint)
        {
            Contents = contents;
            TypeHint = typeHint;
        }

        /// <summary>
        /// Gets the value stored in the slot.
        /// </summary>
        public BitVector Contents
        {
            get;
        }

        /// <summary>
        /// Gets a type hint indicating how this value was pushed.
        /// </summary>
        public StackSlotTypeHint TypeHint
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => $"{Contents} ({TypeHint})";
    }
}