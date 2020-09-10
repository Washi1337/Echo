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

        /// <inheritdoc />
        public int InDegree => Parent is null ? 0 : 1;

        /// <inheritdoc />
        public int OutDegree => GetChildren().Count();

        /// <summary>
        /// Gets the children of the current <see cref="TreeNodeBase"/>
        /// </summary>
        /// <returns>The children of the current <see cref="TreeNodeBase"/></returns>
        public abstract IEnumerable<TreeNodeBase> GetChildren();

        /// <inheritdoc />
        public IEnumerable<IEdge> GetIncomingEdges()
        {
            yield return new Edge(Parent, this);
        }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetOutgoingEdges() =>
            GetChildren().Select(child => (IEdge) new Edge(this, child));

        /// <inheritdoc />
        public IEnumerable<INode> GetPredecessors()
        {
            yield return Parent;
        }

        /// <inheritdoc />
        public IEnumerable<INode> GetSuccessors() => GetChildren();

        /// <inheritdoc />
        public bool HasPredecessor(INode node) => node == Parent;

        /// <inheritdoc />
        public bool HasSuccessor(INode node) => GetChildren().Contains(node);
    }
