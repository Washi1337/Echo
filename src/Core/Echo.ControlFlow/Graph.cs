using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.Core.Graphing;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides a generic base implementation of a control flow graph that contains for each node a user predefined
    /// object in a type safe manner. 
    /// </summary>
    /// <typeparam name="TContents">The type of data that each node in the graph stores.</typeparam>
    public class Graph<TContents> : IGraph
    {
        private Node<TContents> _entrypoint;

        /// <summary>
        /// Creates a new empty graph.
        /// </summary>
        public Graph()
        {
            Nodes = new NodeCollection<TContents>(this);
        }

        /// <summary>
        /// Gets or sets the node that is executed first in the control flow graph.
        /// </summary>
        public Node<TContents> Entrypoint
        {
            get => _entrypoint;
            set
            {
                if (_entrypoint != value)
                {
                    if (!Nodes.Contains(value))
                        throw new ArgumentException("Node is not present in the graph.", nameof(value));
                    _entrypoint = value;
                }
            }
        }

        /// <summary>
        /// Gets a collection of all basic blocks present in the graph.
        /// </summary>
        public NodeCollection<TContents> Nodes
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of all edges that transfer control from one block to the other in the graph.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<Edge<TContents>> GetEdges()
        {
            return Nodes.SelectMany(n => n.GetOutgoingEdges());
        }
        
        IEnumerable<INode> ISubGraph.GetNodes() => Nodes;

        IEnumerable<IEdge> IGraph.GetEdges() => GetEdges();

    }
}