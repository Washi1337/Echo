using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Core.Graphing;
using Echo.DataFlow.Collections;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a single node in a data flow graph.
    /// </summary>
    /// <typeparam name="TContents">The type of contents to store in the node.</typeparam>
    public class DataFlowNode<TContents> : IIdentifiedNode
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
            StackDependencies = new StackDependencyCollection<TContents>(this);
            VariableDependencies = new VariableDependencyCollection<TContents>(this);
        }

        /// <summary>
        /// Gets the data flow graph this node is a part of.
        /// </summary>
        public DataFlowGraph<TContents> ParentGraph
        {
            get;
            internal set;
        }

        /// <inheritdoc />
        public long Id
        {
            get;
            internal set;
        }

        /// <inheritdoc />
        public int InDegree => throw new NotImplementedException();

        /// <inheritdoc />
        public int OutDegree => StackDependencies.EdgeCount + VariableDependencies.EdgeCount;

        /// <summary>
        /// Gets a value indicating whether the data flow node represents an external data source.
        /// </summary>
        public virtual bool IsExternal => false;

        /// <summary>
        /// Gets the contents of the node.
        /// </summary>
        public TContents Contents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of values allocated on a stack that this node depends on.
        /// </summary>
        public StackDependencyCollection<TContents> StackDependencies
        {
            get;
        }

        /// <summary>
        /// Gets a collection of values that are assigned to variables that this node depends on.
        /// </summary>
        public VariableDependencyCollection<TContents> VariableDependencies
        {
            get;
        }

        /// <summary>
        /// Obtains a collection of nodes that depend on this node.
        /// </summary>
        /// <returns>The dependant nodes.</returns>
        public IEnumerable<DataFlowNode<TContents>> GetDependants() => throw new NotImplementedException();

        IEnumerable<IEdge> INode.GetIncomingEdges()
        {
            throw new NotImplementedException();
        }

        IEnumerable<IEdge> INode.GetOutgoingEdges()
        {
            throw new NotImplementedException();
        }

        IEnumerable<INode> INode.GetPredecessors()
        {
            throw new NotImplementedException();
        }

        IEnumerable<INode> INode.GetSuccessors()
        {
            throw new NotImplementedException();
        }

        bool INode.HasPredecessor(INode node)
        {
            throw new NotImplementedException();
        }

        bool INode.HasSuccessor(INode node)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes all incident edges (both incoming and outgoing edges) from the node, effectively isolating the node
        /// in the graph. 
        /// </summary>
        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override string ToString() => $"{Id} ({Contents})";
    }
}