namespace Echo.Core.TypeSystem
{
    /// <summary>
    /// Represents a single field defined in a type.
    /// </summary>
    public interface IField : IMember
    {
        /// <summary>
        /// Gets the type of values that this field stores.
        /// </summary>
        IType FieldType
        {
            get;
        }
        
        /// <summary>
        /// Gets a value indicating whether this field requires an instance to be accessed or not.
        /// </summary>
        bool IsStatic
        {
            get;
        }
        
        /// <summary>
        /// Gets an offset indicating where this field is stored in memory, relative to the starting address
        /// of the containing object.
        /// </summary>
        int Offset
        {
            get;
        }
    }
}