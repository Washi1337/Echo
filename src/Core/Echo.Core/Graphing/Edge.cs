namespace Echo.Core.Graphing
{
    /// <summary>
    /// Provides a basic implementation of an edge in a graph.
    /// </summary>
    public readonly struct Edge : IEdge
    {
        /// <summary>
        /// Creates a new edge in a graph.
        /// </summary>
        /// <param name="origin">The node that this edge starts at in the directed graph.</param>
        /// <param name="target">The node that this edge points to in the directed graph.</param>
        public Edge(INode origin, INode target)
        {
            Origin = origin;
            Target = target;
        }

        /// <inheritdoc />
        public INode Origin
        {
            get;
        }

        /// <inheritdoc />
        public INode Target
        {
            get;
        }

        /// <inheritdoc />
        public override string ToString() => Origin.Id + " -> " + Target.Id;
    }
}