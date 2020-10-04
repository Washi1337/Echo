using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.Core.Graphing
{
    /// <summary>
    /// Provides a base contract for nodes that will be used in a tree
    /// </summary>
    public abstract class TreeNodeBase : INode
    {
        private readonly object _lock = new object();
        
        /// <summary>
        /// Initializes a tree node.
        /// </summary>
        protected TreeNodeBase() => Children = new TreeNodeCollection<TreeNodeBase>(this);
        
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
        public int OutDegree => Children.Count();

        /// <inheritdoc />
        public IEnumerable<IEdge> GetIncomingEdges()
        {
            yield return new Edge(Parent, this);
        }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetOutgoingEdges() =>
            Children.Select(child => (IEdge) new Edge(this, child));

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

        /// <summary>
        /// Updates the value and the parent of the <paramref name="child"/> node.
        /// </summary>
        /// <param name="child">The child element to update.</param>
        /// <param name="value">The new value to assign to the <paramref name="child"/>.</param>
        /// <exception cref="InvalidOperationException">When the node already has a parent.</exception>
        protected void UpdateChild(ref TreeNodeBase child, TreeNodeBase value)
        {
            lock (_lock)
            {
                if (value?.Parent is {})
                    throw new InvalidOperationException("Node already has a parent.");
                if (child is {})
                    child.Parent = null;
                child = value;
                if (child is {})
                    child.Parent = this;
            }
        }
    }
}
