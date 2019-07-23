using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Echo.ControlFlow.Collections
{
    /// <summary>
    /// Represents a mutable collection of nodes present in a graph.
    /// </summary>
    /// <typeparam name="TContents">The type of data that is stored in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class NodeCollection<TContents> : ICollection<Node<TContents>>
    {
        private readonly ISet<Node<TContents>> _nodes = new HashSet<Node<TContents>>();
        private readonly Graph<TContents> _owner;

        internal NodeCollection(Graph<TContents> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _nodes.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;
        
        /// <inheritdoc />
        public IEnumerator<Node<TContents>> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(Node<TContents> item)
        {
            if (item.ParentGraph == _owner)
                return;

            if (item.ParentGraph != null)
                throw new ArgumentException("Cannot add a node from another graph.");
            
            if (_nodes.Add(item))
                item.ParentGraph = _owner;
        }

        /// <summary>
        /// Adds a collection of nodes to the graph.
        /// </summary>
        /// <param name="items">The nodes to add.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when at least one node in the provided collection is already added to another graph.
        /// </exception>
        public void AddRange(IEnumerable<Node<TContents>> items)
        {
            var nodes = items.ToArray();
            if (nodes.Any(n => n.ParentGraph != _owner && n.ParentGraph != null))
                throw new ArgumentException("Sequence contains nodes from another graph.");

            _nodes.UnionWith(nodes);
            foreach (var node in nodes)
                node.ParentGraph = _owner;
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            foreach (var node in _nodes.ToArray())
                Remove(node);
        }

        /// <inheritdoc />
        public bool Contains(Node<TContents> item)
        {
            return _nodes.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(Node<TContents>[] array, int arrayIndex)
        {
            _nodes.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(Node<TContents> item)
        {
            if (_nodes.Remove(item))
            {
                item.ParentGraph = null;
                return true;
            }

            return false;
        }
        
    }
}