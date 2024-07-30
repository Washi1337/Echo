using System.Linq;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a collection of data sources for a single stack slot dependency of a node.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to put in a data flow node.</typeparam>
    public class StackDependency<TInstruction> : DataDependency<StackDataSource<TInstruction>, TInstruction>
        where TInstruction : notnull
    {
        /// <summary>
        /// Adds a data source to the dependency, referencing the first stack value produced by the provided node.
        /// </summary>
        /// <param name="node">The node producing the value.</param>
        /// <returns>The stack data source.</returns>
        public StackDataSource<TInstruction> Add(DataFlowNode<TInstruction> node)
        {
            var source = new StackDataSource<TInstruction>(node);
            return Add(source) 
                ? source 
                : this.First(x => x.Equals(source));
        }
        
        /// <summary>
        /// Adds a data source to the dependency, referencing a stack value produced by the provided node.
        /// </summary>
        /// <param name="node">The node producing the value.</param>
        /// <param name="slotIndex">The index of the stack value that was produced by the node.</param>
        /// <returns>The stack data source.</returns>
        public StackDataSource<TInstruction> Add(DataFlowNode<TInstruction> node, int slotIndex)
        {
            var source = new StackDataSource<TInstruction>(node, slotIndex);
            return Add(source) 
                ? source 
                : this.First(x => x.Equals(source));
        }
    }
}