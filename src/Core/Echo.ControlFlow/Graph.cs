using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow;
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
        public Graph()
        {
            Nodes = new NodeCollection<TInstruction>(this);
        }

        /// <summary>
        /// Gets or sets the node that is executed first in the control flow graph.
        /// </summary>        
        public Node<TInstruction> Entrypoint
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of all basic blocks present in the graph.
        /// </summary>
        public NodeCollection<TInstruction> Nodes
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of all edges that transfer control from one block to the other in the graph.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<Edge<TInstruction>> GetEdges()
        {
            return Nodes.SelectMany(n => n.GetOutgoingEdges());
        }

        INode IGraph.Entrypoint => Entrypoint;

        IEnumerable<INode> IGraph.GetNodes() => Nodes;

        IEnumerable<IEdge> IGraph.GetEdges() => GetEdges();
    }
}