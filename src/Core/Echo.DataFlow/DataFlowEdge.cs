using Echo.Core.Graphing;

namespace Echo.DataFlow
{
    internal struct DataFlowEdge<TContents> : IEdge
    {
        public DataFlowEdge(DataFlowNode<TContents> origin, DataFlowNode<TContents> target)
        {
            Origin = origin;
            Target = target;
        }
        
        public DataFlowNode<TContents> Origin
        {
            get;
        }

        INode IEdge.Origin => Origin;

        public DataFlowNode<TContents> Target
        {
            get;
        }

        INode IEdge.Target => Target;
    }
}