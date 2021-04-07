using System;
using Echo.Core.Code;

namespace Echo.DataFlow
{
    public class VariableDataSource<TContents> : DataSource<TContents>
    {
        /// <inheritdoc />
        public VariableDataSource(DataFlowNode<TContents> node, IVariable variable)
            : base(node)
        {
            Variable = variable ?? throw new ArgumentNullException(nameof(variable));
        }

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