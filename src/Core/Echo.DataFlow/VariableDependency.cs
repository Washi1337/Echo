using System;
using System.Linq;
using Echo.Code;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a collection of data sources for a single variable dependency of a node.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to put in a data flow node.</typeparam>
    public class VariableDependency<TContents> : DataDependency<VariableDataSource<TContents>, TContents>
    {
        /// <summary>
        /// Creates a new variable dependency.
        /// </summary>
        /// <param name="variable">The variable to depend on.</param>
        public VariableDependency(IVariable variable)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        }
        
        /// <summary>
        /// Gets the variable that is depended upon.
        /// </summary>
        public IVariable Variable
        {
            get;
        }
        
        /// <summary>
        /// Adds a data source to the dependency, referencing a variable value assigned by the provided node. 
        /// </summary>
        /// <param name="node">The node assigning the value.</param>
        /// <returns>The variable data source.</returns>
        public VariableDataSource<TContents> Add(DataFlowNode<TContents> node)
        {
            var source = new VariableDataSource<TContents>(node, Variable);
            return Add(source)
                ? source 
                : this.First(x => x.Equals(source));
        }
    }
}