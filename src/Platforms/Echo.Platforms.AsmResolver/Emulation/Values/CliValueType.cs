namespace Echo.Platforms.AsmResolver.Emulation.Values
{
    /// <summary>
    /// Provides members for describing all different types of values that can be stored
    /// on the evaluation stack of the Common Language Infrstructure (CLI).
    /// </summary>
    public enum CliValueType
    {
        /// <summary>
        /// Indicates the value is a 32 bit integer.
        /// </summary>
        Int32,
        
        /// <summary>
        /// Indicates the value is a 64 bit integer. 
        /// </summary>
        Int64,
        
        /// <summary>
        /// Indicates the value is a native integer.
        /// </summary>
        NativeInt,
        
        /// <summary>
        /// Indicates the value is a floating point number type F.
        /// </summary>
        F,
        
        /// <summary>
        /// Indicates the value is an object reference type O.
        /// </summary>
        O,
        
        /// <summary>
        /// Indicates the value is a managed pointer type &amp;.
        /// </summary>
        UnmanagedPointer,
        
        /// <summary>
        /// Indicates the value is an unmanaged pointer type *.
        /// </summary>
        ManagedPointer
    }
}