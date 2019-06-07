using System.Collections.Generic;

namespace Echo.Core.TypeSystem
{
    /// <summary>
    /// Represents a single operation that can be performed on a type.
    /// </summary>
    public interface IMethod : IMember
    {
        /// <summary>
        /// Gets a value indicating whether this field requires an instance to be accessed or not.
        /// </summary>
        bool IsStatic
        {
            get;
        }
        
        /// <summary>
        /// Gets the type of values that this method returns.
        /// </summary>
        IType ReturnType
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of parameters that need to be specified in order to invoke the method.
        /// </summary>
        ICollection<IType> Parameters
        {
            get;
        }
    }
}