namespace Echo.Concrete.Values.ReferenceType
{
    /// <summary>
    /// Represents a pointer value.
    /// </summary>
    public interface IPointerValue : IMemoryAccessValue
    {
        /// <summary>
        /// Gets a value indicating whether the pointer is 32 bit or 64 bit wide.
        /// </summary>
        bool Is32Bit
        {
            get;
        }
    }
}