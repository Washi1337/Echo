using System;
using System.Collections.Generic;
using System.Linq;

namespace Echo.Graphing
{
    /// <summary>
    /// Provides a base contract for nodes that will be used in a tree.
    /// </summary>
    public abstract class TreeNodeBase : INode
    {
        private readonly object _lock = new();
        private TreeNodeBase? _parent;

        /// <summary>
        /// The parent of this <see cref="TreeNodeBase"/>
        /// </summary>
        public TreeNodeBase? Parent
        {
            get => _parent;
            internal set
            {
                if (_parent != value)
                {
                    var original = _parent;
                    _parent = value;
                    OnParentChanged(original);
                }
            }
        }

        /// <inheritdoc />
        public int InDegree => Parent is null ? 0 : 1;

        /// <inheritdoc />
        public int OutDegree => GetChildren().Count();

        /// <summary>
        /// Gets the children of the current <see cref="TreeNodeBase"/>.
        /// </summary>
        /// <returns>The children.</returns>
        public abstract IEnumerable<TreeNodeBase> GetChildren();

        /// <inheritdoc />
        public IEnumerable<IEdge> GetIncomingEdges()
        {
            if (Parent is not null)
                yield return new Edge(Parent, this);
        }

        /// <inheritdoc />
        public IEnumerable<IEdge> GetOutgoingEdges() =>
            GetChildren().Select(child => (IEdge) new Edge(this, child));

        /// <inheritdoc />
        public IEnumerable<INode> GetPredecessors()
        {
            if (Parent is not null)
                yield return Parent;
        }

        /// <inheritdoc />
        public IEnumerable<INode> GetSuccessors() => GetChildren();

        /// <inheritdoc />
        public bool HasPredecessor(INode node) => node == Parent;

        /// <inheritdoc />
        public bool HasSuccessor(INode node) => GetChildren().Contains(node);

        /// <summary>
        /// Updates the value and the parent of the <paramref name="child"/> node.
        /// </summary>
        /// <param name="child">The child element to update.</param>
        /// <param name="value">The new value to assign to the <paramref name="child"/>.</param>
        /// <exception cref="InvalidOperationException">When the node already has a parent.</exception>
        protected void UpdateChild<T>(ref T? child, T? value)
            where T : TreeNodeBase
        {
            lock (_lock)
            {
                if (value?.Parent is not null)
                    throw new InvalidOperationException("Node already has a parent.");
                if (child is not null)
                    child.Parent = null;
                child = value;
                if (child is not null)
                    child.Parent = this;
            }
        }

        /// <summary>
        /// Called when the parent of the tree node changes.
        /// </summary>
        /// <param name="old">The original tree node parent.</param>
        protected virtual void OnParentChanged(TreeNodeBase? old)
        {
        }
    }
}