using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Echo.DataFlow.Collections
{
    /// <summary>
    /// Represents a mutable collection of nodes present in a data flow graph.
    /// </summary>
    /// <typeparam name="TContents">The type of data that is stored in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class DataFlowNodeCollection<TContents> : ICollection<DataFlowNode<TContents>>
    {
        private readonly Dictionary<long, DataFlowNode<TContents>> _nodes = new();
        private readonly DataFlowGraph<TContents> _owner;

        internal DataFlowNodeCollection(DataFlowGraph<TContents> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _nodes.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets a node by its identifier.
        /// </summary>
        /// <param name="id">The node identifier.</param>
        public DataFlowNode<TContents> this[long id] => _nodes[id];

        /// <summary>
        /// Creates and adds a new node to the collection of data flow nodes.
        /// </summary>
        /// <param name="id">The unique identifier of the node.</param>
        /// <param name="contents">The contents of the node.</param>
        /// <returns>The created node.</returns>
        public DataFlowNode<TContents> Add(long id, TContents contents)
        {
            var node = new DataFlowNode<TContents>(id, contents);
            Add(node);
            return node;
        }
        
        /// <inheritdoc />
        public void Add(DataFlowNode<TContents> item)
        {
            if (item.ParentGraph == _owner)
                return;
            if (item.ParentGraph != null)
                throw new ArgumentException("Cannot add a node from another graph.");
            if (_nodes.ContainsKey(item.Id))
                throw new ArgumentException($"A node with identifier 0x{item.Id:X8} was already added to the graph.");

            _nodes.Add(item.Id, item);
            item.ParentGraph = _owner;
        }

        /// <summary>
        /// Adds a collection of nodes to the graph.
        /// </summary>
        /// <param name="items">The nodes to add.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when at least one node in the provided collection is already added to another graph.
        /// </exception>
        public void AddRange(IEnumerable<DataFlowNode<TContents>> items)
        {
            var nodes = items.ToArray();

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.ParentGraph != _owner && node.ParentGraph != null)
                    throw new ArgumentException("Sequence contains nodes from another graph.");
                if (_nodes.ContainsKey(node.Id))
                    throw new ArgumentException($"Sequence contains nodes with identifiers that were already added to the graph.");
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                _nodes.Add(node.Id, node);
                node.ParentGraph = _owner;
            }
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            foreach (long node in _nodes.Keys.ToArray())
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
        public bool Contains(DataFlowNode<TContents> item)
        {
            if (item == null)
                return false;
            return _nodes.TryGetValue(item.Id, out var node) && node == item;
        }

        /// <inheritdoc />
        public void CopyTo(DataFlowNode<TContents>[] array, int arrayIndex)
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Synchronizes all offsets of each node with the underlying instructions.
        /// </summary>
        /// <exception cref="InvalidOperationException">Occurs when one or more nodes are in a state that new offsets
        /// cannot be determined. This includes duplicated offsets.</exception>
        /// <remarks>
        /// <para>
        /// Because updating offsets is a relatively expensive task, calls to this method should be delayed as much as
        /// possible.
        /// </para>
        /// <para>
        /// This method will invalidate any enumerators that are enumerating this collection of nodes.
        /// </para>
        /// </remarks>
        public void UpdateIndices()
        {
            var nodes = new Dictionary<long, DataFlowNode<TContents>>(Count);

            // Verify whether all basic blocks are valid, i.e. all offsets can be obtained successfully and contain
            // no duplicate offsets. If any problem arises we do not want to commit any changes to the node collection.
            foreach (var entry in _nodes)
            {
                var node = entry.Value;
                if (node.IsExternal)
                {
                    nodes.Add(entry.Key, entry.Value);
                }
                else
                {
                    long newOffset = _owner.Architecture.GetOffset(node.Contents);
                    if (nodes.ContainsKey(newOffset))
                    {
                        throw new InvalidOperationException(
                            $"Collection contains multiple instances of the offset {newOffset:X8}.");
                    }

                    nodes.Add(newOffset, node);
                }
            }

            // Update the collection by editing the dictionary directly instead of using the public Clear and Add
            // methods. The public methods remove any incident edges to each node, which means we'd have to add them 
            // again. 

            _nodes.Clear();

            foreach (var entry in nodes)
            {
                entry.Value.Id = entry.Key;
                _nodes.Add(entry.Key, entry.Value);
            }
        }

        /// <inheritdoc />
        public bool Remove(DataFlowNode<TContents> item) => 
            item != null && Remove(item.Id);

        /// <inheritdoc />
        public IEnumerator<DataFlowNode<TContents>> GetEnumerator() => _nodes.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
    }
}