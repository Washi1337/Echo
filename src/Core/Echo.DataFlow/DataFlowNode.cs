using System;
using System.Collections.Generic;
using System.Linq;
using Echo.Graphing;
using Echo.DataFlow.Collections;

namespace Echo.DataFlow
{
    /// <summary>
    /// Represents a single node in a data flow graph.
    /// </summary>
    /// <typeparam name="TInstruction">The type of contents to store in the node.</typeparam>
    public class DataFlowNode<TInstruction> : IIdentifiedNode
        where TInstruction : notnull
    {
        /// <summary>
        /// Creates a new data flow graph node.
        /// </summary>
        /// <param name="instruction">The contents of the node.</param>
        public DataFlowNode(TInstruction? instruction)
        {
            Instruction = instruction;
            StackDependencies = new StackDependencyCollection<TInstruction>(this);
            VariableDependencies = new VariableDependencyCollection<TInstruction>(this);
            IncomingEdges = new List<DataFlowEdge<TInstruction>>();
        }

        /// <summary>
        /// Gets the data flow graph this node is a part of.
        /// </summary>
        public DataFlowGraph<TInstruction>? ParentGraph
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
        /// Gets or sets the offset associated to the data flow node.
        /// </summary>
        public long Offset
        {
            get;
            set;
        }

        long IIdentifiedNode.Id => Offset;
        
        /// <summary>
        /// Gets the contents of the node.
        /// </summary>
        public TInstruction? Instruction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection of values allocated on a stack that this node depends on.
        /// </summary>
        public StackDependencyCollection<TInstruction> StackDependencies
        {
            get;
        }

        /// <summary>
        /// Gets a collection of values that are assigned to variables that this node depends on.
        /// </summary>
        public VariableDependencyCollection<TInstruction> VariableDependencies
        {
            get;
        }

        internal List<DataFlowEdge<TInstruction>> IncomingEdges
        {
            get;
        }
        
        /// <summary>
        /// Obtains a collection of edges that refer to dependent nodes.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<DataFlowEdge<TInstruction>> GetIncomingEdges() => IncomingEdges;

        IEnumerable<IEdge> INode.GetIncomingEdges() => IncomingEdges;

        /// <summary>
        /// Obtains a collection of edges encoding all the dependencies that this node has.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<DataFlowEdge<TInstruction>> GetOutgoingEdges()
        {
            return StackDependencies
                .SelectMany(d => d.GetEdges())
                .Concat(VariableDependencies.SelectMany(d => d.GetEdges()));
        }

        IEnumerable<IEdge> INode.GetOutgoingEdges() => GetOutgoingEdges();

        /// <summary>
        /// Obtains a collection of nodes that depend on this node.
        /// </summary>
        /// <returns>The dependant nodes.</returns>
        public IEnumerable<DataFlowNode<TInstruction>> GetDependants() => IncomingEdges
            .Select(e => e.Dependent)
            .Distinct();

        IEnumerable<INode> INode.GetPredecessors() => GetDependants();

        IEnumerable<INode> INode.GetSuccessors() => GetOutgoingEdges()
            .Select(e => e.DataSource.Node)
            .Distinct();

        bool INode.HasPredecessor(INode node) => GetOutgoingEdges().Any(e => e.Dependent == node);

        bool INode.HasSuccessor(INode node) => GetOutgoingEdges().Any(e => e.DataSource.Node == node);

        /// <summary>
        /// Synchronizes the node's offset with the offset of the embedded instruction.
        /// </summary>
        public void UpdateOffset()
        {
            Offset = ParentGraph is not null && Instruction is not null
                ? ParentGraph.Architecture.GetOffset(Instruction)
                : 0;
        }

        /// <summary>
        /// Removes all incident edges (both incoming and outgoing edges) from the node, effectively isolating the node
        /// in the graph. 
        /// </summary>
        public void Disconnect()
        {
            // Remove edges from dependent nodes.
            while (IncomingEdges.Count > 0)
                RemoveIncomingEdge(IncomingEdges[0]);

            // Clear dependency nodes.
            foreach (var dependency in StackDependencies)
                dependency.Clear();
            foreach (var dependency in VariableDependencies)
                dependency.Clear();
        }

        private static void RemoveIncomingEdge(DataFlowEdge<TInstruction> edge)
        {
            switch (edge.DataSource.Type)
            {
                case DataDependencyType.Stack:
                    foreach (var dependency in edge.Dependent.StackDependencies)
                    {
                        if (dependency.Remove((StackDataSource<TInstruction>) edge.DataSource))
                            break;
                    }

                    break;

                case DataDependencyType.Variable:
                    foreach (var dependency in edge.Dependent.VariableDependencies)
                    {
                        if (dependency.Remove((VariableDataSource<TInstruction>) edge.DataSource))
                            break;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override string ToString() => Offset.ToString("X8");
    }
}