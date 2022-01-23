using Echo.Concrete;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    public readonly struct StackSlot
    {
        public StackSlot(BitVector contents, StackSlotTypeHint typeHint)
        {
            Contents = contents;
            TypeHint = typeHint;
        }

        public BitVector Contents
        {
            get;
        }

        public StackSlotTypeHint TypeHint
        {
            get;
        }

        public override string ToString() => $"{Contents} ({TypeHint})";
    }
}