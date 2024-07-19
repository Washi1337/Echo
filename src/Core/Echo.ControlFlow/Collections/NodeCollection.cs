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
    /// <typeparam name="TInstruction">The type of data that is stored in each node.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class NodeCollection<TInstruction> : ICollection<ControlFlowNode<TInstruction>>
        where TInstruction : notnull
    {
        private readonly List<ControlFlowNode<TInstruction>> _nodes = new();
        private readonly ControlFlowGraph<TInstruction> _owner;

        internal NodeCollection(ControlFlowGraph<TInstruction> owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _nodes.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(ControlFlowNode<TInstruction> item)
        {
            if (item is null)
                throw new ArgumentNullException();
            if (item.ParentGraph == _owner)
                return;
            if (item.ParentGraph != null)
                throw new ArgumentException("Cannot add a node from another graph.");

            _nodes.Add(item);
            item.ParentRegion = _owner;
        }

        /// <summary>
        /// Adds a collection of nodes to the graph.
        /// </summary>
        /// <param name="items">The nodes to add.</param>
        /// <exception cref="ArgumentException">
        /// Occurs when at least one node in the provided collection is already added to another graph.
        /// </exception>
        public void AddRange(IEnumerable<ControlFlowNode<TInstruction>> items)
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
                node.ParentRegion = _owner;
            }
        }
        
        /// <inheritdoc />
        public void Clear()
        {
            foreach (var node in _nodes.ToArray())
                Remove(node);
        }

        /// <inheritdoc />
        public bool Contains(ControlFlowNode<TInstruction> item) => _nodes.Contains(item);

        /// <inheritdoc />
        public void CopyTo(ControlFlowNode<TInstruction>[] array, int arrayIndex) => _nodes.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(ControlFlowNode<TInstruction> item)
        {
            if (_nodes.Remove(item))
            {
                // Remove outgoing edges.
                item.UnconditionalEdge = null;
                item.ConditionalEdges.Clear();
                item.AbnormalEdges.Clear();
                
                // Remove incoming edges.
                foreach (var edge in item.IncomingEdges.ToArray())
                {
                    switch (edge.Type)
                    {
                        case ControlFlowEdgeType.Unconditional:
                        case ControlFlowEdgeType.FallThrough:
                            edge.Origin.UnconditionalEdge = null;
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
                
                // Remove node from graph.
                if (item.ParentRegion is not null)
                {
                    item.ParentRegion.RemoveNode(item);
                    item.ParentRegion = null;
                }

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

        public IDictionary<long, ControlFlowNode<TInstruction>> CreateOffsetMap()
        {
            return _nodes.ToDictionary(x => x.Offset, x => x);
        }

        public ControlFlowNode<TInstruction>? GetByOffset(long offset) => _nodes.FirstOrDefault(x => x.Contents.Offset == offset);

        /// <inheritdoc />
        public IEnumerator<ControlFlowNode<TInstruction>> GetEnumerator() => _nodes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
    }
}