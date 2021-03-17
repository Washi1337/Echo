using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Collections;
using Echo.ControlFlow.Regions;
using Echo.Core.Code;
using Echo.Core.Graphing;

namespace Echo.ControlFlow
{
    /// <summary>
    /// Provides a generic base implementation of a control flow graph that contains for each node a user predefined
    /// object in a type safe manner. 
    /// </summary>
    /// <typeparam name="TInstruction">The type of data that each node in the graph stores.</typeparam>
    public class ControlFlowGraph<TInstruction> : IGraph, IControlFlowRegion<TInstruction>
    {
        private ControlFlowNode<TInstruction> _entrypoint;

        /// <summary>
        /// Creates a new empty graph.
        /// </summary>
        /// <param name="architecture">The architecture description of the instructions stored in the control flow graph.</param>
        public ControlFlowGraph(IInstructionSetArchitecture<TInstruction> architecture)
        {
            Architecture = architecture ?? throw new ArgumentNullException(nameof(architecture));
            Nodes = new NodeCollection<TInstruction>(this);
            Regions = new RegionCollection<TInstruction, ControlFlowRegion<TInstruction>>(this);
        }

        /// <summary>
        /// Gets or sets the node that is executed first in the control flow graph.
        /// </summary>
        public ControlFlowNode<TInstruction> Entrypoint
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
        /// Gets the architecture of the instructions that are stored in the control flow graph.
        /// </summary>
        public IInstructionSetArchitecture<TInstruction> Architecture
        {
            get;
        }

        /// <summary>
        /// Gets a collection of all basic blocks present in the graph.
        /// </summary>
        public NodeCollection<TInstruction> Nodes
        {
            get;
        }

        /// <summary>
        /// Gets a collection of top-level regions that this control flow graph defines. 
        /// </summary>
        public RegionCollection<TInstruction, ControlFlowRegion<TInstruction>> Regions
        {
            get;
        }

        /// <inheritdoc />
        ControlFlowGraph<TInstruction> IControlFlowRegion<TInstruction>.ParentGraph => null;

        /// <inheritdoc />
        IControlFlowRegion<TInstruction> IControlFlowRegion<TInstruction>.ParentRegion => null;
        
        /// <summary>
        /// Gets a collection of all edges that transfer control from one block to the other in the graph.
        /// </summary>
        /// <returns>The edges.</returns>
        public IEnumerable<ControlFlowEdge<TInstruction>> GetEdges() => 
            Nodes.SelectMany(n => n.GetOutgoingEdges());

        IEnumerable<IEdge> IGraph.GetEdges() => GetEdges();

        /// <inheritdoc />
        public ControlFlowNode<TInstruction> GetNodeByOffset(long offset) => Nodes[offset];

        /// <inheritdoc />
        IEnumerable<ControlFlowNode<TInstruction>> IControlFlowRegion<TInstruction>.GetNodes() => Nodes;

        /// <inheritdoc />
        IEnumerable<INode> ISubGraph.GetNodes() => Nodes;

        /// <inheritdoc />
        IEnumerable<ISubGraph> ISubGraph.GetSubGraphs() => Regions;

        ControlFlowNode<TInstruction> IControlFlowRegion<TInstruction>.GetEntrypoint() => Entrypoint;

        /// <inheritdoc />
        IEnumerable<ControlFlowRegion<TInstruction>> IControlFlowRegion<TInstruction>.GetSubRegions() => Regions;
        
        /// <inheritdoc />
        bool IControlFlowRegion<TInstruction>.RemoveNode(ControlFlowNode<TInstruction> node) => 
            Nodes.Remove(node);

        /// <inheritdoc />
        IEnumerable<ControlFlowNode<TInstruction>> IControlFlowRegion<TInstruction>.GetSuccessors() =>
            Enumerable.Empty<ControlFlowNode<TInstruction>>();
    }
}