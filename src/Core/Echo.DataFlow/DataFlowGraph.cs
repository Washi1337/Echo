using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Code;
using Echo.Core.Graphing;
using Echo.DataFlow.Collections;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a graph that encodes data dependencies between objects. An edge (A, B) indicates node A depends on
    /// the evaluation of node B.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to store for each node.</typeparam>
    public class DataFlowGraph<TContents> : IGraph
    {
        /// <summary>
        /// Creates a new data flow graph.
        /// </summary>
        public DataFlowGraph(IInstructionSetArchitecture<TContents> architecture)
        {
            Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
            Nodes = new DataFlowNodeCollection<TContents>(this);
        }

        /// <summary>
        /// Gets the architecture of the instructions that are stored in the data flow graph.
        /// </summary>
        public IInstructionSetArchitecture<TContents> Architecture
        {
            get;
        }
        
        /// <summary>
        /// Gets a collection of nodes that are present in the graph.
        /// </summary>
        public DataFlowNodeCollection<TContents> Nodes
        {
            get;
        }

        INode ISubGraph.GetNodeById(long id) => Nodes[id];

        IEnumerable<INode> ISubGraph.GetNodes() => Nodes;

        /// <inheritdoc />
        public IEnumerable<IEdge> GetEdges()
        {
            return Nodes
                .Cast<INode>()
                .SelectMany(n => n.GetOutgoingEdges());
        }
    }
}