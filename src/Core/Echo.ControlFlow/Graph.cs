using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.Core.Code;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides a base implementation of a control flow graph, that stores for each node a list of instructions.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to use.</typeparam>
    public class Graph<TInstruction> : IGraph
        where TInstruction : IInstruction
    {
        private Node<TInstruction> _entrypoint;

        public Graph()
        {
            Nodes = new NodeCollection<TInstruction>(this);
        }

        /// <summary>
        /// Gets or sets the node that is executed first in the control flow graph.
        /// </summary>
        public Node<TInstruction> Entrypoint
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
        public NodeCollection<TInstruction> Nodes
        {
            get;
        }

        /// <summary>
        /// Gets an ordered collection of all exception handlers that are defined in the control flow graph.
        /// </summary>
        /// <remarks>
        /// If two exception handlers are nested (i.e. they overlap in the try segments), the one that occurs first in
        /// this collection is the enclosing exception handler.
        /// </remarks>
        public IList<ExceptionHandler<TInstruction>> ExceptionHandlers
        {
            get;
        } = new List<ExceptionHandler<TInstruction>>();
        
        /// <summary>
        /// Gets a collection of all edges that transfer control from one block to the other in the graph.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<Edge<TInstruction>> GetEdges()
        {
            return Nodes.SelectMany(n => n.GetOutgoingEdges());
        }

        public Node<TInstruction> GetNodeByOffset(long offset)
        {
            // TODO: use something more efficient than a linear search.
            return Nodes.FirstOrDefault(n => n.Instructions.Count > 0 && n.Instructions[0].Offset == offset);
        }
        
        INode IGraphSegment.Entrypoint => Entrypoint;

        IEnumerable<INode> IGraphSegment.GetNodes() => Nodes;

        IEnumerable<IEdge> IGraph.GetEdges() => GetEdges();

        IEnumerable<IExceptionHandler> IGraph.GetExceptionHandlers() => ExceptionHandlers;
    }
}