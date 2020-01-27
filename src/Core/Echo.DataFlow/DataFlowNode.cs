using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;
using Echo.DataFlow.Collections;
using Echo.DataFlow.Values;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a single node in a data flow graph.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to store in the node.</typeparam>
    public class DataFlowNode<TContents> : IDataFlowNode
    {
        /// <summary>
        /// Creates a new data flow graph node.
        /// </summary>
        /// <param name="id">A unique identifier for the node that can be used for indexing the node.</param>
        /// <param name="contents">The contents of the node.</param>
        public DataFlowNode(long id, TContents contents)
        {
            Id = id;
            Contents = contents;
            StackDependencies = new DependencyCollection<TContents>(this);
        }

        /// <summary>
        /// Gets the data flow graph this node is a part of.
        /// </summary>
        public DataFlowGraph<TContents> ParentGraph
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the unique identifier for the node, used for indexing the node.
        /// </summary>
        public long Id
        {
            get;
        }

        /// <summary>
        /// Gets the contents of the node.
        /// </summary>
        public TContents Contents
        {
            get;
        }

        /// <summary>
        /// Gets a collection of values allocated on a stack that this node depends on.
        /// </summary>
        public DependencyCollection<TContents> StackDependencies
        {
            get;
        }

        internal IList<DataFlowNode<TContents>> Dependants
        {
            get;
        } = new List<DataFlowNode<TContents>>();

        IEnumerable<IEdge> INode.GetIncomingEdges()
        {
            foreach (var d in Dependants)
                yield return new DataFlowEdge<TContents>(d, this);
        }

        IEnumerable<IEdge> INode.GetOutgoingEdges()
        {
            foreach (var dataSource in StackDependencies.SelectMany(v => v.DataSources))
                yield return new DataFlowEdge<TContents>(this, dataSource);
        }

        IEnumerable<INode> INode.GetPredecessors() => Dependants;

        IEnumerable<INode> INode.GetSuccessors() => StackDependencies.SelectMany(v => v.DataSources);

        bool INode.HasPredecessor(INode node) => Dependants.Contains(node);

        bool INode.HasSuccessor(INode node) => StackDependencies.Any(dep => dep.DataSources.Contains(node));

        IEnumerable<ISymbolicValue> IDataFlowNode.GetStackDependencies() => StackDependencies;

        /// <inheritdoc />
        public override string ToString() => $"{Id} ({Contents})";
    }
}