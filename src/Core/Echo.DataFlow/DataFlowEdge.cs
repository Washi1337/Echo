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
        
        /// <summary>
        /// Gets the node that this edge starts at in the directed graph.
        /// </summary>
        public DataFlowNode<TContents> Origin
        {
            get;
        }

        INode IEdge.Origin => Origin;

        /// <summary>
        /// Gets the node that this edge points to in the directed graph.
        /// </summary>
        public DataFlowNode<TContents> Target
        {
            get;
        }

        INode IEdge.Target => Target;

        /// <inheritdoc />
        public override string ToString() => $"{Origin.Id} -> {Target.Id}";
    }
}