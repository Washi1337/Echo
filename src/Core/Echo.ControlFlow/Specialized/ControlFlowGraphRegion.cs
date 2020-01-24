using System;
using System.Collections.Generic;
using System.Linq;
using Echo.ControlFlow.Specialized.Blocks;
using Echo.Core.Code;
using Echo.Core.Graphing;

namespace Echo.ControlFlow.Specialized
{
    /// <summary>
    /// Provides a base implementation of a segment in a control flow graph that stores for each node a list of instructions.
    /// </summary>
    /// <typeparam name="TInstruction">The type of instructions to store in the nodes.</typeparam>
    public class ControlFlowGraphRegion<TInstruction> : ISubGraph
    {
        private Node<BasicBlock<TInstruction>> _entrypoint;

        /// <summary>
        /// Gets the node that is executed first when executing the segment.
        /// </summary>
        public Node<BasicBlock<TInstruction>> Entrypoint
        {
            get => _entrypoint;
            set
            {
                if (_entrypoint != value)
                {
                    if (!Nodes.Contains(value))
                        throw new ArgumentException("Provided node is not present in the graph segment.");
                    _entrypoint = value;
                }
            }
        }

        /// <summary>
        /// Gets a collection of nodes that this segment contains.
        /// </summary>
        /// <returns>The nodes.</returns>
        public ISet<Node<BasicBlock<TInstruction>>> Nodes
        {
            get;
        } = new HashSet<Node<BasicBlock<TInstruction>>>();

        IEnumerable<INode> ISubGraph.GetNodes() => Nodes;
    }
}