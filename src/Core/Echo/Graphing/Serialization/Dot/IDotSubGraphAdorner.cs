using System.Collections.Generic;

namespace Echo.Graphing.Serialization.Dot
{
    /// <summary>
    /// Provides members for adorning a sub graph.
    /// </summary>
    public interface IDotSubGraphAdorner
    {
        /// <summary>
        /// Determines the name of the provided sub graph.
        /// </summary>
        /// <param name="subGraph">The sub graph.</param>
        /// <returns></returns>
        string GetSubGraphName(ISubGraph subGraph);
        
        /// <summary>
        /// Obtains the adornments that should be added to the sub graph. 
        /// </summary>
        /// <param name="subGraph">The sub graph to adorn.</param>
        /// <returns>The adornments.</returns>
        IDictionary<string, string> GetSubGraphAttributes(ISubGraph subGraph);
    }
}