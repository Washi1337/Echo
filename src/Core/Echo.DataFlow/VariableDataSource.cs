using System;
using Echo.Core.Code;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a data source that refers to a variable value assigned by a node in a data flow graph.
    /// </summary>
    /// <typeparam name="TContents">The type of data stored in each data flow node.</typeparam>
    public class VariableDataSource<TContents> : DataSource<TContents>
    {
        /// <summary>
        /// Creates a new variable data source referencing a variable value assigned by the provided node. 
        /// </summary>
        /// <param name="node">The node assigning the value.</param>
        /// <param name="variable">The variable that was assigned a value.</param>
        public VariableDataSource(DataFlowNode<TContents> node, IVariable variable)
            : base(node)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        }

        /// <summary>
        /// Gets the variable that was referenced by <see cref="DataSource{TContents}.Node"/>.
        /// </summary>
        public IVariable Variable
        {
            get;
        }

        /// <inheritdoc />
        public override DataDependencyType Type => DataDependencyType.Variable;

        /// <inheritdoc />
        public override string ToString() => $"{Node.Id:X8}:{Variable.Name}";
    }
}