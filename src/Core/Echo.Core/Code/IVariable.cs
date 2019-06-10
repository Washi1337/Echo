namespace Echo.Core.Code
{
    /// <summary>
    /// Represents a single variable in a virtual machine.
    /// </summary>
    public interface IVariable
    {
        /// <summary>
        /// Gets the name of the variable.
        /// </summary>
        string Name
        {
            get;
        }
    }
    
}