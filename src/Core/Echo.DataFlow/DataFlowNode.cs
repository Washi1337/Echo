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
            IncomingEdges = new List<DataFlowEdge<TContents>>();
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
        public int InDegree => IncomingEdges.Count;

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

        internal List<DataFlowEdge<TContents>> IncomingEdges
        {
            get;
        }
        
        /// <summary>
        /// Obtains a collection of edges that refer to dependent nodes.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<DataFlowEdge<TContents>> GetIncomingEdges() => IncomingEdges;

        IEnumerable<IEdge> INode.GetIncomingEdges() => IncomingEdges;

        /// <summary>
        /// Obtains a collection of edges encoding all the dependencies that this node has.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<DataFlowEdge<TContents>> GetOutgoingEdges()
        {
            return StackDependencies
                .SelectMany(d => d.GetEdges())
                .Concat(VariableDependencies.Values.SelectMany(d => d.GetEdges()));
        }

        IEnumerable<IEdge> INode.GetOutgoingEdges() => GetOutgoingEdges();

        /// <summary>
        /// Obtains a collection of nodes that depend on this node.
        /// </summary>
        /// <returns>The dependant nodes.</returns>
        public IEnumerable<DataFlowNode<TContents>> GetDependants()=> IncomingEdges
            .Select(e => e.Dependent)
            .Distinct();
        
        IEnumerable<INode> INode.GetPredecessors() => GetDependants();

        IEnumerable<INode> INode.GetSuccessors() => GetOutgoingEdges()
            .Select(e => e.DataSource.Node)
            .Distinct();

        bool INode.HasPredecessor(INode node) => GetOutgoingEdges().Any(e => e.Dependent == node);

        bool INode.HasSuccessor(INode node) => GetOutgoingEdges().Any(e => e.DataSource.Node == node);

        /// <summary>
        /// Removes all incident edges (both incoming and outgoing edges) from the node, effectively isolating the node
        /// in the graph. 
        /// </summary>
        public void Disconnect()
        {
            // Remove edges from dependent nodes.
            while (IncomingEdges.Count > 0)
            {
                var edge = IncomingEdges[0];
                switch (edge.DataSource.Type)
                {
                    case DataDependencyType.Stack:
                        foreach (var dependency in edge.Dependent.StackDependencies)
                            dependency.Remove(this);
                        break;

                    case DataDependencyType.Variable:
                        foreach (var entry in edge.Dependent.VariableDependencies)
                            entry.Value.Remove(this);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            // Clear dependency nodes.
            foreach (var dependency in StackDependencies)
                dependency.Clear();
            foreach (var entry in VariableDependencies)
                entry.Value.Clear();
        }

        /// <inheritdoc />
        public override string ToString() => $"{Id} ({Contents})";
    }
}