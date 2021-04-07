using Echo.Core.Graphing;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents an edge between two nodes in a data flow graph (DFG). The origin of the node represents the dependant,
    /// and the target of the node represents the dependency.
    /// </summary>
    /// <typeparam name="TContents">The type of information to store in each data flow node.</typeparam>
    public class DataFlowEdge<TContents> : IEdge
    {
        /// <summary>
        /// Creates a new dependency edge between two nodes.
        /// </summary>
        /// <param name="dependent">The dependent node.</param>
        /// <param name="target">The dependency node.</param>
        /// <param name="type">The type of dependency.</param>
        public DataFlowEdge(DataFlowNode<TContents> dependent, DataSource<TContents> target, DataDependencyType type)
        {
            Dependent = dependent;
            DataSource = target;
            Type = type;
        }
        
        /// <summary>
        /// Gets node that depends on the data source. 
        /// </summary>
        public DataFlowNode<TContents> Dependent
        {
            get;
        }
        
        INode IEdge.Origin => Dependent;

        /// <summary>
        /// Gets the data source this data flow edge points to.
        /// </summary>
        public DataSource<TContents> DataSource
        {
            get;
        }

        INode IEdge.Target => DataSource.Node;

        /// <summary>
        /// Gets the type of dependency that this edge encodes.
        /// </summary>
        public DataDependencyType Type
        {
            get;
        }
        
    }
}