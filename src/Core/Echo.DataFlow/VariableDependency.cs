using System.Linq;
using Echo.Core.Code;

namespace Echo.DataFlow
{
    public class VariableDependency<TContents> : DataDependency<VariableDataSource<TContents>, TContents>
    {
        /// <inheritdoc />
        public VariableDependency(IVariable variable)
        {
            Variable = variable;
        }
        
        public IVariable Variable
        {
            get;
        }
        
        public VariableDataSource<TContents> Add(DataFlowNode<TContents> node)
        {
            var source = new VariableDataSource<TContents>(node, Variable);
            if (Add(source))
                return source;
            return this.First(x => x.Equals(source));
        }
    }
}