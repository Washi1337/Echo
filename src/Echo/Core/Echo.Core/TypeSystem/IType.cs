using System.Collections.Generic;

namespace Echo.Core.TypeSystem
{
    /// <summary>
    /// Represents a single type in a type system of a platform.
    /// </summary>
    public interface IType : IMember
    {
        /// <summary>
        /// Gets the base type that this type extends.
        /// </summary>
        IType BaseType
        {
            get;
        }       
        
        /// <summary>
        /// Gets a value indicating values of this type are passed by value or by reference.
        /// </summary>
        bool IsValueType
        {
            get;
        }
        
        /// <summary>
        /// Gets a value indicating whether this type is an interface.
        /// </summary>
        bool IsInterface
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of fields that this type defines.
        /// </summary>
        ICollection<IField> Fields
        {
            get;
        }

        /// <summary>
        /// Gets a collection of operations that this type defines.
        /// </summary>
        ICollection<IMethod> Methods
        {
            get;
        }

        /// <summary>
        /// Gets a collection of generic parameters associated to this type (if any).
        /// </summary>
        ICollection<IType> GenericParameters
        {
            get;
        }
        
    }
}