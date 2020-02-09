namespace Echo.DataFlow
{
    /// <summary>
    /// Provides members for describing types of dependencies between nodes in a data flow graph.
    /// </summary>
    public enum DataDependencyType
    {
        /// <summary>
        /// Indicates the dependency is a stack dependency.
        /// </summary>
        Stack,
        
        /// <summary>
        /// Indicates the dependency is a variable dependency.
        /// </summary>
        Variable
    }
}