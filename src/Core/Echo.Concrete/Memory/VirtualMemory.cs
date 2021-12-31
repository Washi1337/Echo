using Echo.Core.Code;

namespace Echo.Concrete.Memory
{
    public class VirtualMemory : IMemorySpace
    {
        public AddressRange AddressRange
        {
            get;
        }

        public bool IsValidAddress(long address)
        {
            throw new System.NotImplementedException();
        }

        public void Read(long address, BitVectorSpan buffer)
        {
            throw new System.NotImplementedException();
        }

        public void Write(long address, BitVectorSpan buffer)
        {
            throw new System.NotImplementedException();
        }
    }
}