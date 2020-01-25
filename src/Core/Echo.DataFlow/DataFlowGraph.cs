using System.Collections.Generic;
using System.Linq;
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
        public DataFlowGraph()
        {
            Nodes = new DataFlowNodeCollection<TContents>(this);
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

        IEnumerable<IEdge> IGraph.GetEdges()
        {
            return Nodes
                .Cast<INode>()
                .SelectMany(n => n.GetOutgoingEdges());
        }
    }
}