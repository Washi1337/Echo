using Echo.Graphing;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents an edge between two nodes in a data flow graph (DFG). The origin of the node represents the dependant,
    /// and the target of the node represents the dependency.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in each data flow node.</typeparam>
    public class DataFlowEdge<TInstruction> : IEdge
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new dependency edge between two nodes.
        /// </summary>
        /// <param name="dependent">The dependent node.</param>
        /// <param name="target">The dependency node.</param>
        public DataFlowEdge(DataFlowNode<TInstruction> dependent, DataSource<TInstruction> target)
        {
            Dependent = dependent;
            DataSource = target;
        }
        
        /// <summary>
        /// Gets node that depends on the data source. 
        /// </summary>
        public DataFlowNode<TInstruction> Dependent
        {
            get;
        }
        
        INode IEdge.Origin => Dependent;

        /// <summary>
        /// Gets the data source this data flow edge points to.
        /// </summary>
        public DataSource<TInstruction> DataSource
        {
            get;
        }

        INode IEdge.Target => DataSource.Node;
    }
}