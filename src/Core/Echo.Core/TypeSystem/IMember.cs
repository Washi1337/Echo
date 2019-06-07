namespace Echo.Core.TypeSystem
{
    /// <summary>
    /// Represents a single member in a type system.
    /// </summary>
    public interface IMember
    {
        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// Gets the type that declares this member.
        /// </summary>
        IType DeclaringType
        {
            get;
        }
    }
    
}