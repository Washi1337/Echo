using Echo.Core.Code;

namespace Echo.Concrete.Memory
{
    public class BasicMemorySpace : IMemorySpace
    {
        public BasicMemorySpace(int size, bool initialize)
            : this(new BitVector(size * 8, initialize))
        {
        }
        
        public BasicMemorySpace(byte[] backBuffer)
        {
            BackBuffer = new BitVector(backBuffer);
        }
        
        public BasicMemorySpace(BitVector backBuffer)
        {
            BackBuffer = backBuffer;
        }

        public BitVector BackBuffer
        {
            get;
        }

        public AddressRange AddressRange => new(0, BackBuffer.Count / 8);

        public bool IsValidAddress(long address) => AddressRange.Contains(address);

        public void Read(long address, BitVectorSpan buffer)
        {
            BackBuffer.AsSpan((int) (address * 8), buffer.Count).CopyTo(buffer);
        }

        public void Write(long address, BitVectorSpan buffer)
        {
            buffer.CopyTo(BackBuffer.AsSpan((int) (address * 8), buffer.Count));
        }
    }
}