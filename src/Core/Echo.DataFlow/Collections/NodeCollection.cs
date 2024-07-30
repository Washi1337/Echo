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
    /// <typeparam name="TInstruction">The type of instruction that is stored in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class NodeCollection<TInstruction> : ICollection<DataFlowNode<TInstruction>>
        where TInstruction : notnull
    {
        private readonly List<DataFlowNode<TInstruction>> _nodes = new();
        private readonly DataFlowGraph<TInstruction> _owner;

        internal NodeCollection(DataFlowGraph<TInstruction> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _nodes.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Creates and adds a new node to the collection of data flow nodes.
        /// </summary>
        /// <param name="contents">The contents of the node.</param>
        /// <returns>The created node.</returns>
        public DataFlowNode<TInstruction> Add(TInstruction contents)
        {
            var node = new DataFlowNode<TInstruction>(contents);
            Add(node);
            return node;
        }
        
        /// <inheritdoc />
        public void Add(DataFlowNode<TInstruction> item)
        {
            if (item.ParentGraph == _owner)
                return;
            if (item.ParentGraph is not null)
                throw new ArgumentException("Cannot add a node from another graph.");

            _nodes.Add(item);
            item.ParentGraph = _owner;
        }

        /// <summary>
        /// Adds a collection of nodes to the graph.
        /// </summary>
        /// <param name="items">The nodes to add.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when at least one node in the provided collection is already added to another graph.
        /// </exception>
        public void AddRange(IEnumerable<DataFlowNode<TInstruction>> items)
        {
            var nodes = items.ToArray();

            // Validate before adding.
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node.ParentGraph is not null && node.ParentGraph != _owner)
                    throw new ArgumentException("Sequence contains nodes from another graph.");
            }

            // Add the nodes.
            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                _nodes.Add(node);
                node.ParentGraph = _owner;
            }
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            foreach (var node in _nodes.ToArray())
                Remove(node);
        }
        
        /// <inheritdoc />
        public bool Contains(DataFlowNode<TInstruction> item) => _nodes.Contains(item);

        /// <inheritdoc />
        public void CopyTo(DataFlowNode<TInstruction>[] array, int arrayIndex) => _nodes.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(DataFlowNode<TInstruction> item)
        {
            if (_nodes.Remove(item))
            {
                item.Disconnect();
                item.ParentGraph = null;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Synchronizes all offsets of each node and basic blocks with the underlying instructions.
        /// </summary>
        public void UpdateOffsets()
        {
            foreach (var node in _nodes)
                node.UpdateOffset();
        }

        /// <summary>
        /// Constructs a mapping from instruction offsets to their respective nodes.
        /// </summary>
        /// <returns>The mapping</returns>
        /// <exception cref="ArgumentException">The control flow graph contains nodes with duplicated offsets.</exception>
        public IDictionary<long, DataFlowNode<TInstruction>> CreateOffsetMap()
        {
            return _nodes
                .Where(x => !x.IsExternal)
                .ToDictionary(x => x.Offset, x => x);
        }

        /// <summary>
        /// Finds a node by its instruction offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>The node, or <c>null</c> if no node was found with the provided offset.</returns>
        /// <remarks>
        /// This is a linear lookup. For many lookups by offset, consider first creating an offset map using
        /// <see cref="CreateOffsetMap"/>.
        /// </remarks>
        public DataFlowNode<TInstruction>? GetByOffset(long offset) => _nodes.FirstOrDefault(x => x.Offset == offset);

        /// <inheritdoc />
        public IEnumerator<DataFlowNode<TInstruction>> GetEnumerator() => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}