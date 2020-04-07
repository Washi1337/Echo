using Echo.Core.Values;

namespace Echo.Concrete.Values
{
    /// <summary>
    /// Provides a base for all virtualized concrete values.
    /// </summary>
    public interface IConcreteValue : IValue
    {
        /// <summary>
        /// Gets a value indicating whether the object is passed on by value or by reference. 
        /// </summary>
        bool IsValueType
        {
            get;
        }
    }
}