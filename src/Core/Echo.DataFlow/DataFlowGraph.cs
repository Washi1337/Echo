using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Echo.Code;
using Echo.Graphing;
using Echo.Graphing.Serialization.Dot;
using Echo.DataFlow.Collections;
using Echo.DataFlow.Serialization.Dot;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a graph that encodes data dependencies between instructions.
    /// An edge (A, B) indicates node A depends on the evaluation of node B.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instruction to store in each node.</typeparam>
    public class DataFlowGraph<TInstruction> : IGraph
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new data flow graph.
        /// </summary>
        public DataFlowGraph(IArchitecture<TInstruction> architecture)
        {
            Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
            Nodes = new NodeCollection<TInstruction>(this);
        }

        /// <summary>
        /// Gets the architecture of the instructions that are stored in the data flow graph.
        /// </summary>
        public IArchitecture<TInstruction> Architecture
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of nodes that are present in the graph.
        /// </summary>
        public NodeCollection<TInstruction> Nodes
        {
            get;
        }

        /// <inheritdoc />
        IEnumerable<INode> ISubGraph.GetNodes() => Nodes;
        
        /// <inheritdoc />
        IEnumerable<ISubGraph> ISubGraph.GetSubGraphs() => Enumerable.Empty<ISubGraph>();

        /// <inheritdoc />
        public IEnumerable<IEdge> GetEdges()
        {
            return Nodes
                .Cast<INode>()
                .SelectMany(n => n.GetOutgoingEdges());
        }

        /// <summary>
        /// Serializes the data flow graph to the provided output text writer using the dot file format.
        /// </summary>
        /// <param name="writer">The output stream.</param>
        /// <remarks>
        /// To customize the look of the resulting dot file graph, use the <see cref="DotWriter"/> class
        /// instead of this function.
        /// </remarks>
        public void ToDotGraph(TextWriter writer)
        {
            var dotWriter = new DotWriter(writer)
            {
                NodeIdentifier = new IdentifiedNodeIdentifier(),
                NodeAdorner = new DataFlowNodeAdorner<TInstruction>(),
                EdgeAdorner = new DataFlowEdgeAdorner<TInstruction>()
            };
            dotWriter.Write(this);
        }
    }
}