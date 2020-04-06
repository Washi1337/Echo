using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Echo.ControlFlow.Blocks;

namespace Echo.ControlFlow.Collections
{
    /// <summary>
    /// Represents a mutable collection of nodes present in a graph.
    /// </summary>
    /// <typeparam name="TContents">The type of data that is stored in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class NodeCollection<TContents> : ICollection<ControlFlowNode<TContents>>
    {
        private readonly Dictionary<long, ControlFlowNode<TContents>> _nodes = new Dictionary<long, ControlFlowNode<TContents>>();
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
            if (item is null)
                throw new ArgumentNullException();
            if (item.ParentGraph == _owner)
                return;
            if (item.ParentGraph != null)
                throw new ArgumentException("Cannot add a node from another graph.");
            if (_nodes.ContainsKey(item.Offset))
                throw new ArgumentException($"A node with offset 0x{item.Offset:X8} was already added to the graph.");

            _nodes.Add(item.Offset, item);
            item.ParentRegion = _owner;
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
                node.ParentRegion = _owner;
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
                item.ParentRegion.RemoveNode(item);
                item.ParentRegion = null;
                
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public bool Remove(ControlFlowNode<TContents> item) => 
            item != null && Remove(item.Offset);

        /// <summary>
        /// Synchronizes all offsets of each node and basic blocks with the underlying instructions.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when one or more basic blocks referenced by the nodes
        /// are in a state that new offsets cannot be determined. This includes empty basic blocks and duplicated header
        /// offsets.</exception>
        /// <remarks>
        /// <para>
        /// Because updating offsets is a relatively expensive task, calls to this method should be delayed as much as
        /// possible.
        /// </para>
        /// <para>
        /// This method will invalidate any enumerators that are enumerating this collection of nodes.
        /// </para>
        /// </remarks>
        public void UpdateOffsets()
        {
            var nodes = new Dictionary<long, ControlFlowNode<TContents>>(Count);

            // Verify whether all basic blocks are valid, i.e. are not empty and contain no duplicate offsets.
            // If any problem arises we do not want to commit any changes to the node collection.
            foreach (var entry in _nodes)
            {
                var node = entry.Value;

                if (node.Contents.IsEmpty)
                    throw new InvalidOperationException("Collection contains one or more empty basic blocks.");

                long newOffset = _owner.Architecture.GetOffset(node.Contents.Header);
                if (nodes.ContainsKey(newOffset))
                    throw new InvalidOperationException($"Collection contains multiple basic blocks with header offset {newOffset:X8}.");

                nodes.Add(newOffset, node);
            }

            // Update the collection by editing the dictionary directly instead of using the public Clear and Add
            // methods. The public methods remove any incident edges to each node, which means we'd have to add them 
            // again. 

            _nodes.Clear();

            foreach (var entry in nodes)
            {
                entry.Value.Offset = entry.Key;
                entry.Value.Contents.Offset = entry.Key;
                _nodes.Add(entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Obtains an enumerator that enumerates all nodes in the collection.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<ControlFlowNode<TContents>> IEnumerable<ControlFlowNode<TContents>>.GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Represents an enumerator that enumerates all nodes in a control flow graph.
        /// </summary>
        public struct Enumerator : IEnumerator<ControlFlowNode<TContents>>
        {
            private Dictionary<long, ControlFlowNode<TContents>>.Enumerator _enumerator;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> structure.
            /// </summary>
            /// <param name="collection">The collection to enumerate.</param>
            public Enumerator(NodeCollection<TContents> collection)
            {
                _enumerator = collection._nodes.GetEnumerator();
            }

            /// <inheritdoc />
            public ControlFlowNode<TContents> Current => _enumerator.Current.Value;

            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext() => _enumerator.MoveNext();

            /// <inheritdoc />
            public void Reset() => ((IEnumerator) _enumerator).Reset();

            /// <inheritdoc />
            public void Dispose() => _enumerator.Dispose();
        }
        
    }
}