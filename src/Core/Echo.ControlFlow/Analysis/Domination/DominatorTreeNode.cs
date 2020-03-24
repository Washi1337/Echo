using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;

namespace Echo.ControlFlow.Analysis.Domination
{
    /// <summary>
    /// Represents a single node in a dominator tree of a graph. 
    /// </summary>
    /// <remarks>
    /// It stores the original control flow graph node from which this tree node was inferred, as well a reference to its
    /// immediate dominator, and the node it immediate dominates.
    /// </remarks>
    public class DominatorTreeNode : INode
    {
        internal DominatorTreeNode(INode node)
        {
            OriginalNode = node;
            Children  = new DominatorTreeNodeCollection<INode>(this);
        }

        /// <inheritdoc />
        public long Id => OriginalNode.Id;

        /// <inheritdoc />
        int INode.InDegree => Parent is null ? 0 : 1;

        /// <inheritdoc />
        int INode.OutDegree => Children.Count;

        /// <summary>
        /// Gets the node that this tree node was derived from. 
        /// </summary>
        public INode OriginalNode
        {
            get;
        }

        /// <summary>
        /// Gets the parent of the tree node. That is, the immediate dominator of the original control flow graph node.
        /// </summary>
        public DominatorTreeNode Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a collection of child nodes that are immediately dominated by the original control flow graph node.
        /// </summary>
        public IList<DominatorTreeNode> Children
        {
            get;
        }

        /// <summary>
        /// Gets a collection of children representing all nodes that were dominated by the original node, as well as an
        /// immediate successor of the original node.
        /// </summary>
        /// <returns>The children, represented by the dominator tree nodes.</returns>
        public IEnumerable<DominatorTreeNode> GetDirectChildren()
        {
            return Children.Where(c => c.OriginalNode.HasPredecessor(OriginalNode));
        }

        /// <summary>
        /// Gets a collection of children representing all nodes that were dominated by the original node, but were not
        /// an immediate successor of the original node.
        /// </summary>
        /// <returns>The children, represented by the dominator tree nodes.</returns>
        public IEnumerable<DominatorTreeNode> GetIndirectChildren()
        {
            return Children.Where(c => !c.OriginalNode.HasPredecessor(OriginalNode));
        }

        /// <summary>
        /// Gets all the nodes that are dominated by this control flow graph node.
        /// </summary>
        /// <returns>The nodes that are dominated by this node.</returns>
        /// <remarks>
        /// Because of the nature of dominator analysis, this also includes the current node.
        /// </remarks>
        public IEnumerable<DominatorTreeNode> GetDominatedNodes()
        {
            var stack = new Stack<DominatorTreeNode>();
            stack.Push(this);
            
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                yield return current;

                foreach (var child in current.Children)
                    stack.Push(child);
            }
        }

        IEnumerable<IEdge> INode.GetIncomingEdges()
        {
            return new IEdge[]
            {
                new Edge(Parent, this)
            };
        }

        IEnumerable<IEdge> INode.GetOutgoingEdges()
        {
            foreach (var child in Children)
                yield return new Edge(this, child);
        }

        IEnumerable<INode> INode.GetPredecessors()
        {
            return new INode[]
            {
                Parent
            };
        }

        IEnumerable<INode> INode.GetSuccessors() => Children;

        bool INode.HasPredecessor(INode node) => node == Parent;

        bool INode.HasSuccessor(INode node) => Children.Contains(node);
    }
}