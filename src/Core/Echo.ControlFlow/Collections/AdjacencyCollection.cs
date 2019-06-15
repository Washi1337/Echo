using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Echo.Core.Code;

namespace Echo.ControlFlow.Collections
{
    /// <summary>
    /// Represents a collection of edges originating from a single node.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions that each node stores.</typeparam>
    [DebuggerDisplay("Count = {" + nameof(Count) + "}")]
    public class AdjacencyCollection<TInstruction> : ICollection<Edge<TInstruction>>
        where TInstruction : IInstruction
    {
        private readonly IDictionary<INode, ICollection<Edge<TInstruction>>> _neighbours 
            = new Dictionary<INode, ICollection<Edge<TInstruction>>>();

        private readonly EdgeType _edgeType;

        internal AdjacencyCollection(Node<TInstruction> owner, EdgeType edgeType)
        {
            _edgeType = edgeType;
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
        }

        /// <inheritdoc />
        public int Count => _neighbours.Values.Sum(x => x.Count);

        /// <inheritdoc />
        public bool IsReadOnly => false;
        
        /// <summary>
        /// Gets the node that all edges are originating from.
        /// </summary>
        public Node<TInstruction> Owner
        {
            get;
        }

        /// <summary>
        /// Creates and adds a edge to the provided node.
        /// </summary>
        /// <param name="neighbour">The new neighbouring node.</param>
        /// <returns>The created edge.</returns>
        public Edge<TInstruction> Add(Node<TInstruction> neighbour)
        {
            var edge = new Edge<TInstruction>(Owner, neighbour, _edgeType);
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
        public Edge<TInstruction> Add(Edge<TInstruction> edge)
        {
            AssertEdgeValidity(Owner, _edgeType, edge);
            GetEdges(edge.Target).Add(edge);
            edge.Target.IncomingEdges.Add(edge);
            return edge;
        }

        /// <inheritdoc />
        void ICollection<Edge<TInstruction>>.Add(Edge<TInstruction> edge)
        {
            Add(edge);
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var item in this.ToArray())
                Remove(item);
        }

        /// <summary>
        /// Determines whether a node is a neighbour of the current node. That is, determines whether there exists
        /// at least one edge between the current node and the provided node.
        /// </summary>
        /// <param name="neighbour">The node to check.</param>
        /// <returns><c>True</c> if the provided node is a neighbour, <c>false</c> otherwise.</returns>
        public bool Contains(Node<TInstruction> neighbour)
        {
            return GetEdges(neighbour).Count > 0;
        }

        /// <inheritdoc />
        public bool Contains(Edge<TInstruction> item)
        {
            return GetEdges(item.Target).Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(Edge<TInstruction>[] array, int arrayIndex)
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
        public bool Remove(Node<TInstruction> neighbour)
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
        public bool Remove(Edge<TInstruction> edge)
        {
            bool result = GetEdges(edge.Target).Remove(edge);
            if (result)
                edge.Target.IncomingEdges.Remove(edge);
            
            return result;
        }
        
        /// <inheritdoc />
        public IEnumerator<Edge<TInstruction>> GetEnumerator()
        {
            return _neighbours
                .SelectMany(x => x.Value)
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal static void AssertEdgeValidity(Node<TInstruction> owner, EdgeType type, Edge<TInstruction> item)
        {
            if (item.Type != type)
            {
                throw new ArgumentException(
                    $"Cannot add an edge of type {item.Type} to a collection of type {type}.");
            }
            
            if (item.Source != owner)
                throw new ArgumentException("Cannot add an edge originating from a different node.");
        }

        private ICollection<Edge<TInstruction>> GetEdges(INode target)
        {
            if (!_neighbours.TryGetValue(target, out var edges))
            {
                edges = new HashSet<Edge<TInstruction>>();
                _neighbours[target] = edges;
            }

            return edges;
        }
        
    }
}