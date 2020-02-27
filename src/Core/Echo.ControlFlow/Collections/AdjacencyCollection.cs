using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Echo.Core.Code;
using Echo.Core.Graphing;

namespace Echo.ControlFlow.Collections
{
    /// <summary>
    /// Represents a collection of edges originating from a single node.
    /// </summary>
    /// <typeparam name="TContents">The type of data that each node stores.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class AdjacencyCollection<TContents> : ICollection<ControlFlowEdge<TContents>>
    {
        private readonly Dictionary<INode, HashSet<ControlFlowEdge<TContents>>> _neighbours 
            = new Dictionary<INode, HashSet<ControlFlowEdge<TContents>>>();

        private readonly ControlFlowEdgeType _edgeType;
        private int _count;
        
        internal AdjacencyCollection(ControlFlowNode<TContents> owner, ControlFlowEdgeType edgeType)
        {
            _edgeType = edgeType;
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _count;

        /// <inheritdoc />
        public bool IsReadOnly => false;
        
        /// <summary>
        /// Gets the node that all edges are originating from.
        /// </summary>
        public ControlFlowNode<TContents> Owner
        {
            get;
        }

        /// <summary>
        /// Creates and adds a edge to the provided node.
        /// </summary>
        /// <param name="neighbour">The new neighbouring node.</param>
        /// <returns>The created edge.</returns>
        public ControlFlowEdge<TContents> Add(ControlFlowNode<TContents> neighbour)
        {
            var edge = new ControlFlowEdge<TContents>(Owner, neighbour, _edgeType);
            Add(edge);
            return edge;
        }

        /// <summary>
        /// Adds an edge to the adjacency collection.
        /// </summary>
        /// <param name="edge">The edge to add.</param>
        /// <returns>The edge that was added.</returns>
        /// <exception cref="ArgumentException">
        /// Occurs when the provided edge cannot be added to this collection because of an invalid source node or edge type.
        /// </exception>
        public ControlFlowEdge<TContents> Add(ControlFlowEdge<TContents> edge)
        {
            AssertEdgeValidity(Owner, _edgeType, edge);
            GetEdges(edge.Target).Add(edge);
            edge.Target.IncomingEdges.Add(edge);
            _count++;
            return edge;
        }

        /// <inheritdoc />
        void ICollection<ControlFlowEdge<TContents>>.Add(ControlFlowEdge<TContents> edge)
        {
            Add(edge);
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var item in this.ToArray())
                Remove(item);
            _count = 0;
        }

        /// <summary>
        /// Determines whether a node is a neighbour of the current node. That is, determines whether there exists
        /// at least one edge between the current node and the provided node.
        /// </summary>
        /// <param name="neighbour">The node to check.</param>
        /// <returns><c>True</c> if the provided node is a neighbour, <c>false</c> otherwise.</returns>
        public bool Contains(ControlFlowNode<TContents> neighbour)
        {
            return GetEdges(neighbour).Count > 0;
        }

        /// <inheritdoc />
        public bool Contains(ControlFlowEdge<TContents> item)
        {
            return GetEdges(item.Target).Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(ControlFlowEdge<TContents>[] array, int arrayIndex)
        {
            foreach (var edges in _neighbours.Values)
            {
                edges.CopyTo(array, arrayIndex);
                arrayIndex += edges.Count;
            }
        }
        
        /// <summary>
        /// Removes all edges originating from the current node to the provided neighbour.
        /// </summary>
        /// <param name="neighbour">The neighbour to cut ties with.</param>
        /// <returns><c>True</c> if at least one edge was removed, <c>false</c> otherwise.</returns>
        public bool Remove(ControlFlowNode<TContents> neighbour)
        {
            var edges = GetEdges(neighbour);
            if (edges.Count > 0)
            {
                foreach (var edge in edges.ToArray())
                    Remove(edge);
                return true;
            }

            return false;
        }
        
        /// <inheritdoc />
        public bool Remove(ControlFlowEdge<TContents> edge)
        {
            bool result = GetEdges(edge.Target).Remove(edge);
            if (result)
            {
                _count--;
                edge.Target.IncomingEdges.Remove(edge);
            }

            return result;
        }

        internal static void AssertEdgeValidity(ControlFlowNode<TContents> owner, ControlFlowEdgeType type, ControlFlowEdge<TContents> item)
        {
            if (item.Type != type)
            {
                throw new ArgumentException(
                    $"Cannot add an edge of type {item.Type} to a collection of type {type}.");
            }
            
            if (item.Origin != owner)
                throw new ArgumentException("Cannot add an edge originating from a different node.");
        }

        private ICollection<ControlFlowEdge<TContents>> GetEdges(INode target)
        {
            if (!_neighbours.TryGetValue(target, out var edges))
            {
                edges = new HashSet<ControlFlowEdge<TContents>>();
                _neighbours[target] = edges;
            }

            return edges;
        }
        
        /// <summary>
        /// Obtains an enumerator that enumerates all nodes in the collection.
        /// </summary>
        /// <returns></returns>
        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<ControlFlowEdge<TContents>> IEnumerable<ControlFlowEdge<TContents>>.GetEnumerator() =>
            GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Represents an enumerator that enumerates all nodes in a control flow graph.
        /// </summary>
        public struct Enumerator : IEnumerator<ControlFlowEdge<TContents>>
        {
            private Dictionary<INode, HashSet<ControlFlowEdge<TContents>>>.Enumerator _groupEnumerator;
            private HashSet<ControlFlowEdge<TContents>>.Enumerator _itemIterator;
            private bool _hasItemIterator;
            private ControlFlowEdge<TContents> _current;

            /// <summary>
            /// Creates a new instance of the <see cref="Enumerator"/> structure.
            /// </summary>
            /// <param name="collection">The collection to enumerate.</param>
            public Enumerator(AdjacencyCollection<TContents> collection)
            {
                _groupEnumerator = collection._neighbours.GetEnumerator();
                _itemIterator = default;
                _hasItemIterator = false;
                _current = null;
            }

            /// <inheritdoc />
            public ControlFlowEdge<TContents> Current => _current;

            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public bool MoveNext()
            {
                while (true)
                {
                    if (!_hasItemIterator)
                    {
                        if (!_groupEnumerator.MoveNext())
                            return false;

                        _itemIterator = _groupEnumerator.Current.Value.GetEnumerator();
                        _hasItemIterator = true;
                    }

                    if (_itemIterator.MoveNext())
                    {
                        _current = _itemIterator.Current;
                        return true;
                    }

                    _hasItemIterator = false;
                }
            }

            /// <inheritdoc />
            public void Reset() => ((IEnumerator) _groupEnumerator).Reset();

            /// <inheritdoc />
            public void Dispose() => _groupEnumerator.Dispose();
        }
    }
}