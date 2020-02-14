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
    public class NodeCollection<TContents> : ICollection<ControlFlowNode<TContents>>
    {
        private readonly IDictionary<long, ControlFlowNode<TContents>> _nodes = new Dictionary<long, ControlFlowNode<TContents>>();
        private readonly ControlFlowGraph<TContents> _owner;

        internal NodeCollection(ControlFlowGraph<TContents> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _nodes.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a node by its offset.
        /// </summary>
        /// <param name="offset">The node offset.</param>
        public ControlFlowNode<TContents> this[long offset] => _nodes[offset];

        /// <inheritdoc />
        public void Add(ControlFlowNode<TContents> item)
        {
            if (item.ParentGraph == _owner)
                return;
            if (item.ParentGraph != null)
                throw new ArgumentException("Cannot add a node from another graph.");
            if (_nodes.ContainsKey(item.Offset))
                throw new ArgumentException($"A node with offset 0x{item.Offset:X8} was already added to the graph.");

            _nodes.Add(item.Offset, item);
            item.ParentGraph = _owner;
        }

        /// <summary>
        /// Adds a collection of nodes to the graph.
        /// </summary>
        /// <param name="items">The nodes to add.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when at least one node in the provided collection is already added to another graph.
        /// </exception>
        public void AddRange(IEnumerable<ControlFlowNode<TContents>> items)
        {
            var nodes = items.ToArray();

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.ParentGraph != _owner && node.ParentGraph != null)
                    throw new ArgumentException("Sequence contains nodes from another graph.");
                if (_nodes.ContainsKey(node.Offset))
                    throw new ArgumentException($"Sequence contains nodes with offsets that were already added to the graph.");
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                _nodes.Add(node.Offset, node);
                node.ParentGraph = _owner;
            }
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            foreach (var node in _nodes.Keys.ToArray())
                Remove(node);
        }

        /// <summary>
        /// Determines whether a node with a specific offset was added to the collection.
        /// </summary>
        /// <param name="offset">The offset to the node.</param>
        /// <returns><c>true</c> if there exists a node with the provided offset, <c>false</c> otherwise.</returns>
        public bool Contains(long offset)
        {
            return _nodes.ContainsKey(offset);
        }

        /// <inheritdoc />
        public bool Contains(ControlFlowNode<TContents> item)
        {
            if (item == null)
                return false;
            return _nodes.TryGetValue(item.Offset, out var node) && node == item;
        }

        /// <inheritdoc />
        public void CopyTo(ControlFlowNode<TContents>[] array, int arrayIndex)
        {
            _nodes.Values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes a node by its offset.
        /// </summary>
        /// <param name="offset">The offset. of the node to remove.</param>
        /// <returns><c>true</c> if the collection contained a node with the provided offset., and the node was removed
        /// successfully, <c>false</c> otherwise.</returns>
        public bool Remove(long offset)
        {
            if (_nodes.TryGetValue(offset, out var item))
            {
                // Remove outgoing edges.
                item.FallThroughEdge = null;
                item.ConditionalEdges.Clear();
                item.AbnormalEdges.Clear();
                
                // Remove incoming edges.
                foreach (var edge in item.IncomingEdges.ToArray())
                {
                    switch (edge.Type)
                    {
                        case ControlFlowEdgeType.FallThrough:
                            edge.Origin.FallThroughEdge = null;
                            break;
                        case ControlFlowEdgeType.Conditional:
                            edge.Origin.ConditionalEdges.Remove(edge);
                            break;
                        case ControlFlowEdgeType.Abnormal:
                            edge.Origin.AbnormalEdges.Remove(edge);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                //Remove node.
                _nodes.Remove(offset);
                item.ParentGraph = null;
                
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool Remove(ControlFlowNode<TContents> item) => 
            item != null && Remove(item.Offset);

        /// <inheritdoc />
        public IEnumerator<ControlFlowNode<TContents>> GetEnumerator() => _nodes.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
    }
}