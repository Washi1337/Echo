using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Echo.ControlFlow.Collections
{
    /// <summary>
    /// Represents a collection of edges originating from a single node.
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node stores.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class AdjacencyCollection<TInstruction> : ICollection<ControlFlowEdge<TInstruction>>
        where TInstruction : notnull
    {
        private readonly Dictionary<ControlFlowNode<TInstruction>, HashSet<ControlFlowEdge<TInstruction>>> _neighbours = new();

        private int _count;

        internal AdjacencyCollection(ControlFlowNode<TInstruction> owner, ControlFlowEdgeType edgeType)
        {
            EdgeType = edgeType;
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the type of edges that are stored in this collection.
        /// </summary>
        public ControlFlowEdgeType EdgeType
        {
            get;
        }

        /// <summary>
        /// Gets the node that all edges are originating from.
        /// </summary>
        public ControlFlowNode<TInstruction> Owner
        {
            get;
        }

        /// <summary>
        /// Creates and adds a edge to the provided node.
        /// </summary>
        /// <param name="neighbour">The new neighbouring node.</param>
        /// <returns>The created edge.</returns>
        public ControlFlowEdge<TInstruction> Add(ControlFlowNode<TInstruction> neighbour)
        {
            var edge = new ControlFlowEdge<TInstruction>(Owner, neighbour, EdgeType);
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
        public ControlFlowEdge<TInstruction> Add(ControlFlowEdge<TInstruction> edge)
        {
            AssertEdgeValidity(Owner, edge, EdgeType);
            GetEdges(edge.Target).Add(edge);
            edge.Target.IncomingEdges.Add(edge);
            _count++;
            return edge;
        }

        /// <inheritdoc />
        void ICollection<ControlFlowEdge<TInstruction>>.Add(ControlFlowEdge<TInstruction> edge)
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
        public bool Contains(ControlFlowNode<TInstruction> neighbour)
        {
            return GetEdges(neighbour).Count > 0;
        }

        /// <inheritdoc />
        public bool Contains(ControlFlowEdge<TInstruction> item)
        {
            return GetEdges(item.Target).Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(ControlFlowEdge<TInstruction>[] array, int arrayIndex)
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
        public bool Remove(ControlFlowNode<TInstruction> neighbour)
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
        public bool Remove(ControlFlowEdge<TInstruction> edge)
        {
            bool result = GetEdges(edge.Target).Remove(edge);
            if (result)
            {
                _count--;
                edge.Target.IncomingEdges.Remove(edge);
            }

            return result;
        }

        internal static void AssertEdgeValidity(
            ControlFlowNode<TInstruction> owner, ControlFlowEdge<TInstruction> item, ControlFlowEdgeType type)
        {
            if (item.Type != type)
            {
                throw new ArgumentException(
                    $"Cannot add an edge of type {item.Type} to a collection of type {type}.");
            }
            
            if (item.Origin != owner)
                throw new ArgumentException("Cannot add an edge originating from a different node.");
        }

        /// <summary>
        /// Obtains all edges to the provided neighbour, if any.
        /// </summary>
        /// <param name="target">The neighbouring node.</param>
        /// <returns>The edges.</returns>
        public IEnumerable<ControlFlowEdge<TInstruction>> GetEdgesToNeighbour(ControlFlowNode<TInstruction> target) => 
            GetEdges(target);

        private ICollection<ControlFlowEdge<TInstruction>> GetEdges(ControlFlowNode<TInstruction> target)
        {
            if (!_neighbours.TryGetValue(target, out var edges))
            {
                edges = new HashSet<ControlFlowEdge<TInstruction>>();
                _neighbours[target] = edges;
            }

            return edges;
        }

        /// <inheritdoc />
        public IEnumerator<ControlFlowEdge<TInstruction>> GetEnumerator() =>
            _neighbours.SelectMany(x => x.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}