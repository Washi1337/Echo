namespace Echo.ControlFlow.Analysis.Traversal
{
    public class NodeDiscoveryEventArgs : DiscoveryEventArgs
    {
        public NodeDiscoveryEventArgs(INode newNode, IEdge origin)
        {
            NewNode = newNode;
            Origin = origin;
        }
        
        public INode NewNode
        {
            get;
        }

        public IEdge Origin
        {
            get;
        }
    }
}