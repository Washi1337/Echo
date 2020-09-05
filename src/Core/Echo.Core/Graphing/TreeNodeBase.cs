using System.Collections.Generic;
using System.Linq;

namespace Echo.Core.Graphing
{
    /// <summary>
    /// Provides a base contract for nodes that will be used in a tree
    /// </summary>
    public abstract class TreeNodeBase : INode
    {
        /// <summary>
        /// Initializes a tree node.
        /// </summary>
        protected TreeNodeBase()
        {
            Children = new TreeNodeCollection<TreeNodeBase>(this);
        }
        
        /// <summary>
        /// The parent of this <see cref="TreeNodeBase"/>
        /// </summary>
        public TreeNodeBase Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// The children of this <see cref="TreeNodeBase"/>
        /// </summary>
        public IList<TreeNodeBase> Children
        {
            get;
        }

        /// <inheritdoc />
        public int InDegree => Parent is null ? 0 : 1;

        /// <inheritdoc />
        public int OutDegree => Children.Count;

        /// <inheritdoc />
        public IEnumerable<IEdge> GetIncomingEdges()
        {
            yield return new Edge(Parent, this);
        }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetOutgoingEdges() =>
            Children.Select(child => new Edge(this, child));

        /// <inheritdoc />
        public IEnumerable<INode> GetPredecessors()
        {
            yield return Parent;
        }

        /// <inheritdoc />
        public IEnumerable<INode> GetSuccessors() => Children;

        /// <inheritdoc />
        public bool HasPredecessor(INode node) => node == Parent;

        /// <inheritdoc />
        public bool HasSuccessor(INode node) => Children.Contains(node);
    }
}