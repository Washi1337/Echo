using Echo.Core.Code;

namespace Echo.Concrete.Memory
{
    public interface IMemorySpace
    {
        AddressRange AddressRange
        {
            get;
        }

        bool IsValidAddress(long address);
        
        void Read(long address, BitVectorSpan buffer);
        
        void Write(long address, BitVectorSpan buffer);
    }
}