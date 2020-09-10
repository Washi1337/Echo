using System;

namespace Echo.DataFlow.Analysis
{
    /// <summary>
    /// Provides flags that influence the behaviour of the <see cref="DependencyCollection"/> class.
    /// </summary>
    [Flags]
    public enum DependencyCollectionFlags
    {
        /// <summary>
        /// Indicates stack dependency edges should be traversed during the collection.
        /// </summary>
        IncludeStackDependencies = 1,
        
        /// <summary>
        /// Indicates variable dependency edges should be traversed during the collection.
        /// </summary>
        IncludeVariableDependencies = 2,
        
        /// <summary>
        /// Indicates all edges should be traversed during the collection.
        /// </summary>
        IncludeAllDependencies = IncludeStackDependencies | IncludeVariableDependencies
    }
}